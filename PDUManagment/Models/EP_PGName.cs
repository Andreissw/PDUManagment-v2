
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------


namespace PDUManagment.Models
{

using System;
    using System.Collections.Generic;
    
public partial class EP_PGName
{

    public int ID { get; set; }

    public Nullable<int> IDProtocol { get; set; }

    public string Name { get; set; }

    public Nullable<int> IDMachine { get; set; }

    public Nullable<bool> Visible { get; set; }

    public string Type { get; set; }

    public Nullable<byte> line { get; set; }



    public virtual EP_Machine EP_Machine { get; set; }

    public virtual EP_Protocols EP_Protocols { get; set; }

}

}
