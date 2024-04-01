using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedAlertService.Email
{
    public class EmailRequest
    {
        public int MSALId { get; set; } = 0;
        public int? EventNo { get; set; } = 0;
        public string Subject { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Recipients { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? NonEventNo { get; set; } = 0;
        public string? AttachmentName { get; set; } = string.Empty;

        public byte[]? Attachment { get; set; } = null;

    }
}
