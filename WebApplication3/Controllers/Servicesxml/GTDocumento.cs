using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using WebApplication3.Controllers; // Assuming this is the correct namespace for FacturaModelo and DetalleItem
[XmlRoot("GTDocumento", Namespace = "http://www.sat.gob.gt/dte/fel/0.2.0")]
public class GTDocumento
{
    [XmlElement("SAT")]
    public SAT SAT { get; set; }
}

public class SAT
{
    [XmlElement("DTE")]
    public DTE DTE { get; set; }

    [XmlAttribute("ClaseDocumento")]
    public string ClaseDocumento { get; set; }
}

public class DTE
{
    [XmlElement("DatosEmision")]
    public DatosEmision DatosEmision { get; set; }

    [XmlElement("Certificacion")]
    public Certificacion Certificacion { get; set; }

    [XmlAttribute("ID")]
    public string ID { get; set; }
}

public class DatosEmision
{
    [XmlElement("DatosGenerales")]
    public DatosGenerales DatosGenerales { get; set; }
    [XmlElement("Emisor")]
    public Emisor Emisor { get; set; }
    [XmlElement("Receptor")]
    public ReceptorXML Receptor { get; set; }

    [XmlElement("Items")]
    public Items Items { get; set; }

    [XmlElement("Totales")]
    public Totales Totales { get; set; }
}

public class DatosGenerales
{
    [XmlAttribute("FechaHoraEmision")]
    public DateTime FechaHoraEmision { get; set; }

    [XmlAttribute("CodigoMoneda")]
    public string CodigoMoneda { get; set; }

    [XmlAttribute("NumeroAcceso")]
    public string NumeroAcceso { get; set; }

    [XmlAttribute("Tipo")]
    public string Tipo { get; set; }
}

public class Emisor
{
    [XmlAttribute("NombreEmisor")]
    public string NombreEmisor { get; set; }
    [XmlAttribute("NombreComercial")] // ✅ Este era el problema
    public string NombreComercial { get; set; }
    [XmlAttribute("NITEmisor")]
    public string NITEmisor { get; set; }

    [XmlElement("DireccionEmisor")]
    public Direccion Direccion { get; set; }
}

public class Direccion
{
    [XmlElement("Direccion")]
    public string Calle { get; set; }

    [XmlElement("Municipio")]
    public string Municipio { get; set; }

    [XmlElement("Departamento")]
    public string Departamento { get; set; }

    [XmlElement("Pais")]
    public string Pais { get; set; }
}

public class ReceptorXML
{
    [XmlAttribute("IDReceptor")]
    public string NIT { get; set; }

    [XmlAttribute("NombreReceptor")]
    public string Nombre { get; set; }
}

public class Items
{
    [XmlElement("Item")]
    public List<Itemfel> ItemList { get; set; }
}

public class Itemfel
{
    [XmlAttribute("BienOServicio")]
    public string BienOServicio { get; set; }

    [XmlElement("Cantidad")]
    public decimal Cantidad { get; set; }

    [XmlElement("Descripcion")]
    public string Descripcion { get; set; }

    [XmlElement("PrecioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [XmlElement("Precio")]
    public decimal Precio { get; set; }

    [XmlElement("Descuento")]
    public decimal Descuento { get; set; }

    [XmlElement("Total")]
    public decimal Total { get; set; }

    [XmlElement("Impuestos")]
    public Impuestos Impuestos { get; set; }
}

public class Impuestos
{
    [XmlElement("Impuesto")]
    public List<Impuesto> ImpuestoList { get; set; }
}

public class Impuesto
{
    [XmlElement("NombreCorto")]
    public string NombreCorto { get; set; }

    [XmlElement("MontoImpuesto")]
    public decimal MontoImpuesto { get; set; }
}

public class Totales
{
    [XmlElement("TotalImpuestos")]
    public TotalImpuestos TotalImpuestos { get; set; }

    [XmlElement("GranTotal")]
    public decimal GranTotal { get; set; }
}

public class TotalImpuestos
{
    [XmlElement("TotalImpuesto")]
    public TotalImpuesto TotalImpuesto { get; set; }
}

public class TotalImpuesto
{
    [XmlAttribute("NombreCorto")]
    public string NombreCorto { get; set; }

    [XmlAttribute("TotalMontoImpuesto")]
    public decimal TotalMontoImpuesto { get; set; }
}

public class Certificacion
{
    [XmlElement("NITCertificador")]
    public string NITCertificador { get; set; }

    [XmlElement("NombreCertificador")]
    public string NombreCertificador { get; set; }

    [XmlElement("NumeroAutorizacion")]
    public NumeroAutorizacion NumeroAutorizacion { get; set; }

    [XmlElement("FechaHoraCertificacion")]
    public DateTime FechaHoraCertificacion { get; set; }
}

public class NumeroAutorizacion
{
    [XmlAttribute("Numero")]
    public string Numero { get; set; }

    [XmlAttribute("Serie")]
    public string Serie { get; set; }

    [XmlText]
    public string Autorizacion { get; set; }
}
