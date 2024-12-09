using System;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Text;

class DiskAndProcessorInfo
{
    static void Main(string[] args)
    {
        string report = GenerateReport();

        Console.WriteLine(report);

        SendEmail(report);

        Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
        Console.ReadKey();
    }

    static string GenerateReport()
    {
        StringBuilder report = new StringBuilder();

        report.AppendLine("=== Informacje o przestrzeni na dyskach ===\n");
        try
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    report.AppendLine($"Dysk: {drive.Name}");
                    report.AppendLine($"  Typ: {drive.DriveType}");
                    report.AppendLine($"  System plików: {drive.DriveFormat}");
                    report.AppendLine($"  Całkowita przestrzeń: {drive.TotalSize / (1024.0 * 1024.0 * 1024.0):F2} GB");
                    report.AppendLine($"  Wolna przestrzeń: {drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0):F2} GB");
                    report.AppendLine($"  Dostępna przestrzeń dla użytkownika: {drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0):F2} GB\n");
                }
                else
                {
                    report.AppendLine($"Dysk: {drive.Name} jest niedostępny.\n");
                }
            }
        }
        catch (Exception ex)
        {
            report.AppendLine("Nie udało się uzyskać informacji o dyskach.");
            report.AppendLine($"Błąd: {ex.Message}\n");
        }

        report.AppendLine("\n=== Monitorowanie pracy procesora ===\n");
        try
        {
            float cpuLoad = GetCpuLoad();
            report.AppendLine($"Obciążenie procesora: {cpuLoad:F2}%");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    double temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                    temperature = (temperature - 2732) / 10.0;
                    report.AppendLine($"Temperatura procesora: {temperature:F1} °C");
                }
            }
        }
        catch (Exception ex)
        {
            report.AppendLine("Nie udało się uzyskać danych o pracy procesora.");
            report.AppendLine($"Błąd: {ex.Message}\n");
        }

        return report.ToString();
    }

    static float GetCpuLoad()
    {
        float totalLoad = 0;
        int count = 0;

        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                totalLoad += Convert.ToSingle(obj["LoadPercentage"]);
                count++;
            }
        }

        return count > 0 ? totalLoad / count : 0;
    }

    static void SendEmail(string report)
    {
        try
        {
     
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string senderEmail = "botarkadiusz@gmail.com"; 
            string senderPassword = "omckepbbheypgocg"; 
            string recipientEmail = "adrian.gogacz@zst.radom.pl"; 

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(senderEmail);
            mail.To.Add(recipientEmail);
            mail.Subject = "Raport systemowy";
            mail.Body = report;

            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true; 
            smtpClient.UseDefaultCredentials = false;

            Console.WriteLine("Rozpoczynanie wysyłania wiadomości...");
            smtpClient.Send(mail);

            Console.WriteLine("\nRaport został pomyślnie wysłany na e-mail.");
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine("\nSMTP Exception:");
            Console.WriteLine($"Status Code: {smtpEx.StatusCode}");
            Console.WriteLine($"Message: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nException:");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}
