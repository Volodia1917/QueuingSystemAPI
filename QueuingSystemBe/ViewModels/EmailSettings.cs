﻿namespace QueuingSystemBe.ViewModels
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }
}
