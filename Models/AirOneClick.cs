namespace Api.Web.Dynamics365.Models
{
    public class AirOneClick
    {
        public class DeclaracionDeVentas
        {
            //new_declaraciondeventases
            public string new_declaraciondeventasid { get; set; }
            public string new_fecha { get; set; }
            public string new_cliente { get; set; } //LK Cuenta
            public string new_aeropuerto { get; set; }
            public decimal new_facturacion { get; set; }
            public string transactioncurrencyid { get; set; }
            public decimal new_liquidar { get; set; }
            public int statuscode { get; set; }
        } 

        public class Facturas
        {
            //invoices
            public string invoiceid { get; set; }
            public string name { get; set; }
            public string customerid { get; set; }
            public decimal totalamount { get; set; } //total
            public decimal discountpercentage { get; set; } //descuento
            public string transactioncurrencyid { get; set; }
            public int statuscode { get; set; }
        }
    }
}
