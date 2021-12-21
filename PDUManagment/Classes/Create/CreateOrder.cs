using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment.Classes.Create
{
    public class CreateOrder : CreateClass
    {


        string _path { get; set; }
        string _pathSpec { get; set; }
        string _pathDoc { get; set; }
        [Required]        
        [Min(10)]
        public int Count { get; set; }
        public string mode { get; set; }
        public List<SelectListItem> TypeClient { get; set; }       
        [Required]
        public string ClientType { get; set; }

        public bool IsActive { get; set; }
        public List<SelectListItem> Client { get; set; }
        public string ClientName { get; set; }
        [Required, MinLength(5)]
        public string SpecificationBom { get; set; }
        [MinLength(5)]
        public string Document { get; set; }
        public string DocumentPath { get; set; }     
        [Required]
        public HttpPostedFileBase FileSpec { get; set; }
        public List<HttpPostedFileBase> BOM { get; set; }
        public List<HttpPostedFileBase> Gerbers { get; set; }
        public List<HttpPostedFileBase> PickPlace { get; set; }
        public List<HttpPostedFileBase> AssemblyDrawings { get; set; }
        public List<HttpPostedFileBase> Schematic { get; set; }
        public List<HttpPostedFileBase> Fireware { get; set; }       

        
        public List<DocData> ListDoc = new List<DocData>();

        public override void SetLOG()
        {
            using (FASEntities fas = new FASEntities()) {

                EP_Log log = new EP_Log()
                {
                    IDProtocol = ProtocolID,
                    UserID = (short)UserID,
                    ServiceID = GetServiceID(),
                    Date = DateTime.UtcNow.AddHours(2),
                    Description = $"Клиент: {ClientName} " +
                    $"|Заказ: {Order}  |Спецификация {SpecificationBom}|Сторона платы {TOPBOTName}",
                    IDStep = 4,
                    LOTID = LOTID,

                };

                fas.EP_Log.Add(log);
                fas.SaveChanges();
            }
        }

        public void IdentifyClient()
        {
            ClientName = ClientType == "ВЛВ" ? "ВЛВ" : ClientName;
        }

        public void SaveDoc(List<DocumentFile> Files)
        {
            foreach (var item in Files)
            {
                if (item != null)
                {
                    foreach (var i in item.Files)
                    {
                        if (i == null)
                            continue;

                        _pathDoc = Path.Combine(_path + Path.GetFileName(i.FileName));
                        i.SaveAs(_pathDoc);

                        var fileextresion = _pathDoc.Substring(_pathDoc.LastIndexOf('.'));

                        var contentType = new FASEntities().EP_FileTypeMIME.Where(c => c.FileExtrension == fileextresion).Select(c => c.MIMETypeInternetMediaType).FirstOrDefault();
                        var name = _pathDoc.Substring(_pathDoc.LastIndexOf('\\') + 1);
                       
                        name = name.Substring(0,name.Length - fileextresion.Length);
                        ListDoc.Add(new DocData() { Path = _pathDoc, Name = item.Name, ContentType = contentType, NameFile = name, Extension = fileextresion });                    
                    }
                }
            }
        }

        public void SaveSpec()
        {
           var result = Path.GetFileName(FileSpec.FileName);
           _pathSpec = Path.Combine(_path + result);
           FileSpec.SaveAs(_pathSpec);
           var fileextresion = _pathSpec.Substring(_pathSpec.LastIndexOf('.'));
           
           var contentType = new FASEntities().EP_FileTypeMIME.Where(c => c.FileExtrension == fileextresion).Select(c => c.MIMETypeInternetMediaType).FirstOrDefault();
           var name = _pathSpec.Substring(_pathSpec.LastIndexOf('\\') + 1);
          
           name = name.Substring(0,name.Length - fileextresion.Length);
            ListDoc.Add(new DocData() { Path = _pathSpec, Name = "Спецификация", ContentType = contentType, NameFile = name, Extension = fileextresion });           
        }



        public void CheckFolder()
        {
            string PathObmen = @"\\192.168.180.9\ProdSoft\PDUManagment\";
            //string PathObmen = @"\\192.168.180.9\ProdSoft\PDUManagment\";
            CheckFolder(PathObmen + ClientName);
            CheckFolder(PathObmen + ClientName + "\\" + Order);
            _path = PathObmen + ClientName + "\\" + Order + "\\";
        }

        void CheckFolder(string Path)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }



        public int CreateDocument(DocData docData)
        {

            using (var fas = new FASEntities())
            {
                EP_Doc doc = new EP_Doc()
                {
                    IDProtocol = ProtocolID,
                    Name = docData.Name,
                    Path = docData.Path,
                    ContentType = docData.ContentType,
                    NameFile = docData.NameFile,
                    Visible = true,
                    extension = docData.Extension,
                };

                fas.EP_Doc.Add(doc);
                fas.SaveChanges();

                return doc.ID;
            }

        }

    }

    public class DocumentFile
    { 
        public List<HttpPostedFileBase> Files { get; set; }

        public string Name { get; set; }
    }


}


public class Min : Attribute
{
    public int MinCount { get; set; }
    public Min(int min)
    {
        MinCount = min;
    }

}
