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

        public string ClientType { get; set; }

        public bool IsActive { get; set; }
        public List<SelectListItem> Client { get; set; }

        [Required]
        public string Modelname { get; set; }

        [Required]
        public string ClientName { get; set; }

        [Required]
        public int LotCode { get; set; }

        [Required, MinLength(5)]
        public string SpecificationBom { get; set; }
        [MinLength(5)]
        public string Document { get; set; }
        public string DocumentPath { get; set; }     
        [Required]
        public HttpPostedFileBase FileSpec { get; set; }

        public List<HttpPostedFileBase> BlankOrder { get; set; }
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
                    //IDProtocol = ProtocolID,
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
            //string PathObmen = @"\\192.168.180.9\ProdSoft\PDUManagment\";
            string PathObmen = @"\\s-fs-cts\Obmen\Производство\Производственный_заказ\";
            var path = CheckFolder(PathObmen + ClientName);
            path = CheckFolder(path + "\\" + Date.Year);
            path = CheckFolder(path + "\\" + Date.ToString("MM_MMMM"));
            path = CheckFolder(path + "\\" + Order);
            _path = path + "\\";
        }

        public string CheckFolderArchive(string _path)
        {            
            var path = _path.Substring(0, _path.LastIndexOf("\\"));
            path = path + "\\" + "Архив";
            path = CheckFolder(path);
            path = path + _path.Substring(_path.LastIndexOf("\\"));
            File.Move(_path, path);
            return path;
        }


        string CheckFolder(string Path)
        {
            //if (!Directory.Exists(Path))
            //    Directory.CreateDirectory(Path);

            if (!Directory.Exists(Path))
                new DirectoryInfo(Path).Create();



            //DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            //directoryInfo.cr

            return Path;
        }



        public int CreateDocument(DocData docData)
        {

            using (var fas = new FASEntities())
            {
                EP_Doc doc = new EP_Doc()
                {
                    LOTID = LOTID,
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
