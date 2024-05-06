using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapasPrintingService
{
    /*
     * Clase interna de ChapasPrintingService, que contiene todas las variables globales del programa
     */
    internal class VariablesGlobales
    {

        //LOG
        public static string sLogServiceName = "ChapasPrintingService";
        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool bLogDetallado = ConfigIni.Default.bLogDetallado;                     // Si bLogDetallado = true, recibirimos infromación detallada del servicio.
        public static string TipoOrigen = ConfigIni.Default.TipoOrigen.ToString();              // Nos indica si el directorio origen lo encontramos de forma local o mediante un servidor FTP
        public static string stRutaOrigen = ConfigIni.Default.RutaOrigen.ToString();            // Ruta del directorio Origen
        public static string stRutaDestino = ConfigIni.Default.RutaDestino.ToString();          // Ruta del directorio Destino
        public static string stRutaProcesado = ConfigIni.Default.RutaProcesado.ToString();      // Ruta del directoorio de archivos procesados
        public static string DescProcesado = ConfigIni.Default.DescProcesado.ToString();        // Descripción que recibiran los archivos procesados en el nombre
        public static string NombreDestino = ConfigIni.Default.NombreDestino.ToString();        // Nombre que la maquina láser reconoce para poder imprimirlo
        public static string InfoOrdenacion = ConfigIni.Default.TipoOrdenacion.ToString();      // Tipo de ordenación en la que se enviarán los archivos
        public static int RefreshTime = Int32.Parse(ConfigIni.Default.RefreshTime);             // Tiempo de espera hasta volver a comprobar si el directorio destino acepta un nuevo archivo
        public static string IP = ConfigIni.Default.IP.ToString();                              // IP del servidor FTP
        public static string Usuario = ConfigIni.Default.Usuario.ToString();                    // Usuario del servidor FTP con el que se logueará
        public static string Contrasena = ConfigIni.Default.Contrasena.ToString();              // Contraseña del usuario del servidor FTP
        public static string ExtensionOrigen = ConfigIni.Default.ExtensionOrigen.ToString();    // Extensión de los archivos que se quieren enviar a imprimir


        /*
         * Dependiendo de la variable <InfoOrdenación> se utilizará un comparador u otro
         * Pre: ---
         * Post: Devuelve un comparador de listas de clases <FileInfo> e <InformacionFicheros>, por defecto se ordena de forma alfanumérica ascendente
         */
        public static TiposDeOrdenaciones TipoOrdenacion(string InfoOrdenacion)
        {

            if (InfoOrdenacion.Equals("ModDesc"))
            {
                return new OrdenacionModificacionDesc();
            }
            else if (InfoOrdenacion.Equals("AlfDesc"))
            {
                return new OrdenacionAlfDesc();
            }
            else if (InfoOrdenacion.Equals("CreAsc"))
            {
                return new OrdenacionCreacionAsc();
            }
            else if (InfoOrdenacion.Equals("CreDesc"))
            {
                return new OrdenacionCreacionDesc();
            }
            else if (InfoOrdenacion.Equals("ModAsc"))
            {
                return new OrdenacionModificacionAsc();
            }
            else //if (InfoOrdenacion.Equals("AlfAsc"))
            {              
                return new OrdenacionAlfAsc();
            }

        }
        
        
    }
}
