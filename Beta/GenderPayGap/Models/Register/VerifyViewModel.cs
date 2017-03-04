﻿namespace GenderPayGap.WebUI.Models.Register
{
    public class VerifyViewModel
    {
        public VerifyViewModel()
        {

        }

        public long UserId { get; set; }
        public bool Retry { get; set; }
        public bool Resend { get; set; }
        public string EmailAddress { get; set; }
        public bool Sent { get; internal set; }
    }
}