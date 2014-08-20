using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommissionSystem.Domain.Helpers;
using OfficeOpenXml;
using ProtoBuf;
using NLog;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class CommissionResult
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public CommissionResult()
        {
            CommissionViewDic = new Dictionary<string, List<CommissionView>>();
            AgentViewList = new List<AgentView>();
        }

        [ProtoMember(1)]
        public Dictionary<string, List<CommissionView>> CommissionViewDic { get; set; }
        [ProtoMember(1)]
        public List<AgentView> AgentViewList { get; set; }

        public Attachment GetDataCommissionResultData(DateTime dateFrom, DateTime dateTo)
        {
            ExcelPackage pk = null;
            Attachment att = null;

            try
            {
                pk = new ExcelPackage();
                ExcelWorksheet ws = Utils.CreateSheet(pk, "Commission", 1);
                int row = 1;
                int col = 1;
                int space = 2;

                foreach (AgentView a in AgentViewList)
                {
                    string r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;
                    r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;
                    r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;

                    row -= 3;
                    col = 1;

                    ws.Cells[row++, col].Value = string.Format("Commission Period: {0} - {1}",
                        Utils.FormatDateTime(dateFrom), Utils.FormatDateTime(dateTo));
                    ws.Cells[row++, col].Value = string.Format("Agent: {0}: {1}", a.AgentID, a.AgentName);
                    ws.Cells[row++, col].Value = string.Format("Total Commission Payable: {0}", Utils.FormatCurrency(a.TotalCommission));

                    ++row;

                    if (CommissionViewDic[a.AgentID.ToString()].Count < 1)
                    {
                        row += space;
                        continue;
                    }

                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "No.";
                    ws.Cells[row, col++].Value = "CustID";
                    ws.Cells[row, col++].Value = "Name";
                    ws.Cells[row, col++].Value = "Desc";
                    ws.Cells[row, col].Style.WrapText = false;
                    ws.Cells[row, col++].Value = "Settlement Date";
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "Settlement Amount";
                    ws.Cells[row, col++].Value = "Comm Rate";
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "Comm Amount";

                    ++row;

                    for (int i = 0; i < CommissionViewDic[a.AgentID.ToString()].Count; i++)
                    {
                        col = 1;

                        CommissionView k = CommissionViewDic[a.AgentID.ToString()][i];
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = string.Format("{0}.", i + 1);

                        ws.Cells[row, col].Style.Numberformat.Format = "0";
                        ws.Cells[row, col++].Value = k.Customer.CustID;

                        ws.Cells[row, col++].Value = k.Customer.Name;

                        int j = row;

                        foreach (CustomerBillingInfo bi in k.Customer.BillingInfoList)
                        {
                            ws.Cells[row++, col].Value = bi.ProductType.Description;
                        }

                        if (row > j)
                            row = j;

                        ++col;
                        j = row;

                        foreach (CustomerSettlement se in k.Customer.SettlementList)
                        {
                            ws.Cells[row, col].Style.WrapText = false;
                            ws.Cells[row, col].Value = Utils.FormatDateTime(se.RealDate);
                            ws.Cells[row, col + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            ws.Cells[row++, col + 1].Value = Utils.FormatCurrency(se.Amount);
                        }

                        ++col;

                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = Utils.FormatCurrency(k.SettlementAmount);
                        ws.Cells[row, col++].Value = string.Format("(T x {0})", k.CommissionRate);
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = Utils.FormatCurrency(k.Commission);

                        ++row;
                    }

                    row += space;
                }

                for (int i = 1; i <= 8; i++)
                    ws.Column(i).AutoFit();

                att = new Attachment();
                att.Data = pk.GetAsByteArray();
                att.Filename = "CommissionResult.xlsx";
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (pk != null)
                    pk.Dispose();
            }

            return att;
        }

        public Attachment GetFibrePlusCommissionResultData(DateTime dateFrom, DateTime dateTo)
        {
            ExcelPackage pk = null;
            Attachment att = null;

            try
            {
                pk = new ExcelPackage();
                ExcelWorksheet ws = Utils.CreateSheet(pk, "Commission", 1);
                int row = 1;
                int col = 1;
                int space = 2;

                foreach (AgentView a in AgentViewList)
                {
                    string r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;
                    r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;
                    r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;

                    row -= 3;
                    col = 1;

                    ws.Cells[row++, col].Value = string.Format("Commission Period: {0} - {1}",
                        Utils.FormatDateTime(dateFrom), Utils.FormatDateTime(dateTo));
                    ws.Cells[row++, col].Value = string.Format("Agent: {0} ({1}): {2}", a.AgentID, a.AgentType, a.AgentName);
                    ws.Cells[row++, col].Value = string.Format("Total Commission Payable: {0}", Utils.FormatCurrency(a.TotalCommission));

                    ++row;

                    if (CommissionViewDic[a.AgentID.ToString()].Count < 1)
                    {
                        row += space;
                        continue;
                    }

                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "No.";
                    ws.Cells[row, col++].Value = "CustID";
                    ws.Cells[row, col++].Value = "Name";
                    ws.Cells[row, col++].Value = "Desc";
                    ws.Cells[row, col].Style.WrapText = false;
                    ws.Cells[row, col++].Value = "Settlement Date";
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "Settlement Amount";
                    ws.Cells[row, col++].Value = "Comm Rate";
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "Comm Amount";

                    ++row;

                    for (int i = 0; i < CommissionViewDic[a.AgentID.ToString()].Count; i++)
                    {
                        col = 1;

                        CommissionView k = CommissionViewDic[a.AgentID.ToString()][i];
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = string.Format("{0}.", i + 1);

                        ws.Cells[row, col].Style.Numberformat.Format = "0";
                        ws.Cells[row, col++].Value = k.Customer.CustID;

                        ws.Cells[row, col++].Value = k.Customer.Name;

                        int j = row;

                        foreach (CustomerBillingInfo bi in k.Customer.BillingInfoList)
                        {
                            ws.Cells[row++, col].Value = string.Format("{0} ({1})", bi.ProductType.Description,
                                Utils.FormatCurrency(bi.ProductType.InitialAmount));
                        }

                        if (row > j)
                            --row;

                        ++col;

                        if (k.Customer.SettlementList.Count > 0)
                        {
                            ws.Cells[row, col].Style.WrapText = false;
                            ws.Cells[row, col++].Value = Utils.FormatDateTime(k.Customer.SettlementList.First().RealDate);
                        }

                        else
                            col++;

                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = Utils.FormatCurrency(k.SettlementAmount);
                        ws.Cells[row, col++].Value = string.Format("(T x {0})", k.CommissionRate);
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = Utils.FormatCurrency(k.Commission);

                        ++row;
                    }

                    row += space;
                }

                for (int i = 1; i <= 8; i++)
                    ws.Column(i).AutoFit();

                att = new Attachment();
                att.Data = pk.GetAsByteArray();
                att.Filename = "CommissionResult.xlsx";
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (pk != null)
                    pk.Dispose();
            }

            return att;
        }
    }

    [ProtoContract]
    public class VoiceCommissionResult
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public VoiceCommissionResult()
        {
            CommissionViewDic = new Dictionary<string, List<VoiceCommissionView>>();
            AgentViewList = new List<AgentView>();
        }

        [ProtoMember(1)]
        public Dictionary<string, List<VoiceCommissionView>> CommissionViewDic { get; set; }
        [ProtoMember(2)]
        public List<AgentView> AgentViewList { get; set; }

        public Attachment GetVoiceCommissionResultData(DateTime dateFrom, DateTime dateTo)
        {
            ExcelPackage pk = null;
            Attachment att = null;

            try
            {
                pk = new ExcelPackage();
                ExcelWorksheet ws = Utils.CreateSheet(pk, "Commission", 1);
                int row = 1;
                int col = 1;
                int space = 2;

                foreach (AgentView a in AgentViewList)
                {
                    string r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;
                    r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;
                    r = string.Format("A{0}:D{0}", row++);
                    ws.Cells[r].Merge = true;

                    row -= 3;
                    col = 1;

                    ws.Cells[row++, col].Value = string.Format("Commission Period: {0} - {1}",
                        Utils.FormatDateTime(dateFrom), Utils.FormatDateTime(dateTo));
                    ws.Cells[row++, col].Value = string.Format("Agent: {0}: {1}", a.AgentID, a.AgentName);
                    ws.Cells[row++, col].Value = string.Format("Total Commission Payable: {0}", Utils.FormatCurrency(a.TotalCommission));

                    ++row;

                    if (CommissionViewDic[a.AgentID.ToString()].Count < 1)
                    {
                        row += space;
                        continue;
                    }

                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "No.";
                    ws.Cells[row, col++].Value = "CustID";
                    ws.Cells[row, col++].Value = "Name";
                    ws.Cells[row, col++].Value = "Desc";
                    ws.Cells[row, col].Style.WrapText = false;
                    ws.Cells[row, col++].Value = "Settlement Date";
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "Settlement Amount";
                    ws.Cells[row, col++].Value = "Comm Rate";
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    ws.Cells[row, col++].Value = "Comm Amount";

                    ++row;

                    for (int i = 0; i < CommissionViewDic[a.AgentID.ToString()].Count; i++)
                    {
                        col = 1;

                        CommissionView k = CommissionViewDic[a.AgentID.ToString()][i];
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = string.Format("{0}.", i + 1);

                        ws.Cells[row, col].Style.Numberformat.Format = "0";
                        ws.Cells[row, col++].Value = k.Customer.CustID;

                        ws.Cells[row, col++].Value = k.Customer.Name;

                        int j = row;

                        foreach (CustomerBillingInfo bi in k.Customer.BillingInfoList)
                        {
                            ws.Cells[row++, col].Value = bi.ProductType.Description;
                        }

                        if (row > j)
                            row = j;

                        ++col;
                        j = row;

                        foreach (CustomerSettlement se in k.Customer.SettlementList)
                        {
                            ws.Cells[row, col].Style.WrapText = false;
                            ws.Cells[row, col].Value = Utils.FormatDateTime(se.RealDate);
                            ws.Cells[row, col + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            ws.Cells[row++, col + 1].Value = Utils.FormatCurrency(se.Amount);
                        }

                        ++col;

                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = Utils.FormatCurrency(k.SettlementAmount);
                        ws.Cells[row, col++].Value = string.Format("(T x {0})", k.CommissionRate);
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        ws.Cells[row, col++].Value = Utils.FormatCurrency(k.Commission);

                        ++row;
                    }

                    row += space;
                }

                for (int i = 1; i <= 8; i++)
                    ws.Column(i).AutoFit();

                att = new Attachment();
                att.Data = pk.GetAsByteArray();
                att.Filename = "CommissionResult.xlsx";
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (pk != null)
                    pk.Dispose();
            }

            return att;
        }
    }
}
