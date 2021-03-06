﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class FibrePlusRequest
    {
        public int AgentID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int? Page { get; set; }
        public bool Load { get; set; }
    }
}