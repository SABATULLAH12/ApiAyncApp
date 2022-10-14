using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAyncApp.Models
{
    public class Response
    {
        public bool isCompleted { get; set; }
        public string message { get; set; }
        public bool isError { get; set; }

        public Guid id { get; set; }
        public string url { get; set; }
    }
}
