using System;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Model;
using System.Text;

/// <summary>
/// ZipSendMailService ���ʹ�����ѹ���ļ�
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class SendMailManage : System.Web.Services.WebService 
{
    public static string SendHost = "mail.tcbci.com";
    public static string UserName = "baolong.liu@tcbci.com";
    public static string UserPass = "123456";
    public static string FromText = "������";
    public static string FromEmail = "system@tcbci.com";
    public static string FromTextEN = "Project Leads from TCBCI��test��";
    public SendMailManage() 
    {
        //���ʹ����Ƶ��������ȡ��ע�������� 
        //InitializeComponent(); 
    } 

    /// <summary>
    /// ��ʼ���ʼ����ò���
    /// </summary>
    private static void StartConfig()
    {
        SqlCommand cmd = new SqlCommand("prSendMailManage", objConn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Clear();
        Connection();
        cmd.Parameters.Add("@flag", SqlDbType.TinyInt).Value = 5;
        SqlDataReader dr = cmd.ExecuteReader();
        if (dr.HasRows)
        {
            dr.Read();
            SendHost = dr["SendHost"].ToString();
            UserName = dr["UserName"].ToString();
            UserPass = dr["UserPass"].ToString();
            FromText = dr["FromText"].ToString();
            FromEmail = dr["FromEmail"].ToString();
            FromTextEN = dr["FromTextEN"].ToString();
        }
        dr.Close();
        dr.Dispose();
        cmd.Dispose();
        DisConnection();  
    }

    /// <summary>
    /// �����ʼ�:����Ƕ��ʼ������÷ֺŸ���
    /// </summary>
    [WebMethod]//string Email, string Header, string Content, string AddFileName
    public static bool SendMailAdd(SendMail sm)
    {
        if (!string.IsNullOrEmpty(sm.Email))
        {
            //�����ʼ�����
            StartConfig();
            System.Net.Mail.MailMessage myMail = new System.Net.Mail.MailMessage();
            if (sm.Email.IndexOf(';') == -1)
            {
                myMail = new MailMessage("\"" + FromText + "\"<" + FromEmail + ">", sm.Email.Trim(), sm.Subject, sm.Content);
                if (!string.IsNullOrEmpty(sm.FileName))
                    myMail.Attachments.Add(new Attachment(sm.FileName));
                myMail.IsBodyHtml = true;//����ΪHTML��ʽ
                myMail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");//���ı���
                myMail.SubjectEncoding = System.Text.Encoding.GetEncoding("GB2312");//�������
                System.Net.Mail.SmtpClient mySMTP = new SmtpClient();
                mySMTP.Host = SendHost;
                mySMTP.UseDefaultCredentials = true;
                mySMTP.Credentials = new System.Net.NetworkCredential(UserName, UserPass);
                mySMTP.DeliveryMethod = SmtpDeliveryMethod.Network;//ָ�������ʼ����ͷ�ʽ
                try
                {
                    mySMTP.Send(myMail);
                    if (!string.IsNullOrEmpty(sm.FileName))
                        DeleteFileName(sm.FileName);
                    InsertError(null, sm);
                    return true;
                }
                catch (Exception ex)
                {
                    InsertError(ex, sm);
                }
            }
            else
            {
                //����ʼ�����
                string[] strArr = sm.Email.Split(';');
                for (int i = 0; i < strArr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(strArr[i].Trim()))
                    {
                        myMail = new MailMessage("\"" + FromText + "\"<" + FromEmail + ">", strArr[i].Trim(), sm.Subject, sm.Content);
                        if (!string.IsNullOrEmpty(sm.FileName))
                            myMail.Attachments.Add(new Attachment(sm.FileName));
                        myMail.IsBodyHtml = true;
                        myMail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
                        myMail.SubjectEncoding = System.Text.Encoding.GetEncoding("GB2312");
                        System.Net.Mail.SmtpClient mySMTP = new SmtpClient();
                        mySMTP.Host = SendHost;
                        mySMTP.UseDefaultCredentials = true;
                        mySMTP.Credentials = new System.Net.NetworkCredential(UserName, UserPass);
                        mySMTP.DeliveryMethod = SmtpDeliveryMethod.Network;
                        try
                        {
                            mySMTP.Send(myMail);
                        }
                        catch (Exception ex)
                        {
                            InsertError(ex, sm);
                        }
                    }
                }
                try
                {
                    if (!string.IsNullOrEmpty(sm.FileName))
                        DeleteFileName(sm.FileName);
                    return true;
                }
                catch
                { }
            }
        }
        return false;
        //return true;
    }

    /// <summary>
    /// ������Ӣ���ʼ�
    /// �����ʼ�:����Ƕ��ʼ������÷ֺŸ���
    /// </summary>
    [WebMethod]//string Email, string Header, string Content, string AddFileName
    public static bool SendMailAdd(SendMail sm, int language)
    {
        if (!string.IsNullOrEmpty(sm.Email))
        {
            //�����ʼ�����
            StartConfig();
            System.Net.Mail.MailMessage myMail = new System.Net.Mail.MailMessage();
            if (language == 0)
              myMail = new MailMessage("\"" + FromText + "\"<" + FromEmail + ">", sm.Email.Trim(), sm.Subject, sm.Content);
            else
              myMail = new MailMessage("\"" + FromTextEN + "\"<" + FromEmail + ">", sm.Email.Trim(), sm.Subject, sm.Content);
            if (sm.Email.IndexOf(';') == -1)
            {
                //myMail = new MailMessage("\"" + FromText + "\"<" + FromEmail + ">", sm.Email.Trim(), sm.Subject, sm.Content);
                if (!string.IsNullOrEmpty(sm.FileName))
                    myMail.Attachments.Add(new Attachment(sm.FileName));
                myMail.IsBodyHtml = true;
                myMail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
                myMail.SubjectEncoding = System.Text.Encoding.GetEncoding("GB2312");
                System.Net.Mail.SmtpClient mySMTP = new SmtpClient();
                mySMTP.Host = SendHost;
                mySMTP.UseDefaultCredentials = true;
                mySMTP.Credentials = new System.Net.NetworkCredential(UserName, UserPass);
                mySMTP.DeliveryMethod = SmtpDeliveryMethod.Network;
                try
                {
                    mySMTP.Send(myMail);
                    if (!string.IsNullOrEmpty(sm.FileName))
                        DeleteFileName(sm.FileName);
                    InsertError(null, sm);
                    return true;

                }
                catch (Exception ex)
                {
                    InsertError(ex, sm);
                }
            }
            else
            {
                //����ʼ�����
                string[] strArr = sm.Email.Split(';');
                for (int i = 0; i < strArr.Length; i++)
                {
                    //myMail = new MailMessage("\"" + FromText + "\"<" + FromEmail + ">", strArr[i].Trim(), sm.Subject, sm.Content);
                    if (!string.IsNullOrEmpty(sm.FileName))
                        myMail.Attachments.Add(new Attachment(sm.FileName));
                    myMail.IsBodyHtml = true;
                    myMail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
                    myMail.SubjectEncoding = System.Text.Encoding.GetEncoding("GB2312");
                    System.Net.Mail.SmtpClient mySMTP = new SmtpClient();
                    mySMTP.Host = SendHost;
                    mySMTP.UseDefaultCredentials = true;
                    mySMTP.Credentials = new System.Net.NetworkCredential(UserName, UserPass);
                    mySMTP.DeliveryMethod = SmtpDeliveryMethod.Network;
                    try
                    {
                        mySMTP.Send(myMail);
                        InsertError(null, sm);
                    }
                    catch (Exception ex)
                    {
                        InsertError(ex, sm);
                    }
                }
                try
                {
                    if (!string.IsNullOrEmpty(sm.FileName))
                        DeleteFileName(sm.FileName);
                    return true;
                }
                catch
                { }
            }
        }
        return false;
        //return true;
    }

    /// <summary>
    /// ɾ�����ͳɹ����ʼ��ĸ����ļ�
    /// </summary>
    private static void DeleteFileName(string filename)
    {      
        if (File.Exists(filename.Replace(".zip", ".doc")))
        {
            try
            {
                File.Delete(filename.Replace(".zip", ".doc"));
            }
            catch { }
        }
        else
        {
            try
            {
                File.Delete(filename.Replace(".zip", ".xls"));
            }
            catch { }
        }
    }

    private static void InsertError(Exception ex, SendMail sm)
    {
        StringBuilder sb = new StringBuilder();
        if (ex != null)
        {
            sb.Append(ex.InnerException.Message);
            sb.Append("\n\n");
            sb.Append(ex.InnerException.InnerException.Message);
            sb.Append("\n\n");
            sb.Append(ex.InnerException.StackTrace);
            sb.Append("\n\n\n");
            sb.Append(ex.ToString());
        }

        SendMail smError = new SendMail();
        smError.Email = sm.Email;
        smError.Subject = sm.Subject;
        smError.Error = sb.ToString();
        smError.Content = sm.Content;
        try
        {
            BusinessLogic.SendMailManage.SendMailErrorInsert(smError);
        }
        catch { }

    }

    #region ���ݿ����Ӳ���
    public static SqlConnection objConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Buildnet_DB"].ConnectionString);
    public static void Connection()
    {
        if (objConn == null)
            objConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Buildnet_DB"].ConnectionString);
        if (objConn.State != ConnectionState.Open)
        {
            objConn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Buildnet_DB"].ConnectionString;
            objConn.Open();
        }
    }
    public static void DisConnection()
    {
        if (objConn.State != System.Data.ConnectionState.Closed)
        {
            objConn.Close();
            objConn.Dispose();
        }
    }
   
    #endregion


}










ʹ��

SendMail sm = new SendMail();
sm.Subject = "�쳽TCBCI��ͨ�����ʺ�֪ͨ";
sm.Content = str;
sm.MailType = "�����ʺŷ���";
sm.Email = conMai;
SendMailManage.SendMailAdd(sm);








SendMail ʵ����



using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class SendMail
    {
        private int _mailid;
        private string _email;
        private string _subject;
        private string _content;
        private int _logid;
        private string _title;
        private string _adddate;
        private string _MailType;

        private string _SendHost;
        private string _UserName;
        private string _UserPass;
        private int _Interval;

        private string _fromtext;
        private string _fromemail;
        private string _filename;
        private string _subjectencoding;
        private string _bodyencoding;

        private string _error;



        public string SubjectEncoding
        {
            set { _subjectencoding = value; }
            get
            {
                if (string.IsNullOrEmpty(_subjectencoding))
                    return "GB2312";
                else
                    return _subjectencoding;
            }
        }
        public string BodyEncoding
        {
            set { _bodyencoding = value; }
            get
            {
                if (string.IsNullOrEmpty(_bodyencoding))
                    return "GB2312";
                else
                    return _bodyencoding;
            }
        }
        public string FileName
        {
            set { _filename = value; }
            get { return _filename; }
        }
        public string FromText
        {
            set { _fromtext = value; }
            get { return _fromtext; }
        }

        public string FromEmail
        {
            set { _fromemail = value; }
            get { return _fromemail; }
        }

        public string SendHost
        {
            set { _SendHost = value; }
            get { return _SendHost; }
        }
        public string UserName
        {
            set { _UserName = value; }
            get { return _UserName; }
        }
        public string UserPass
        {
            set { _UserPass = value; }
            get { return _UserPass; }
        }
        public int Interval
        {
            set { _Interval = value; }
            get { return _Interval; }
        }

        public int MailId
        {
            set { _mailid = value; }
            get { return _mailid; }
        }
        public string Email
        {
            set { _email = value; }
            get { return _email; }
        }
        public string Subject
        {
            set { _subject = value; }
            get { return _subject; }
        }
        public string Content
        {
            set { _content = value; }
            get { return _content; }
        }
        public int LogId
        {
            set { _logid = value; }
            get { return _logid; }
        }
        public string Title
        {
            set { _title = value; }
            get { return _title; }
        }
        public string AddDate
        {
            set { _adddate = value; }
            get { return _adddate; }
        }
        public string MailType
        {
            set { _MailType = value; }
            get { return _MailType; }
        }
        public string Error
        {
            set { _error = value; }
            get { return _error; }
        }
        
    }
}














