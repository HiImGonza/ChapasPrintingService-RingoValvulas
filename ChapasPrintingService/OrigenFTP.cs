using FluentFTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChapasPrintingService
{
    internal class FTP
    {
        public static bool Error = false;       // Vaiable que indica si ha habido algún error no recuperable en el proceso
        public static List<string> ArchivosNoEliminados = new List<string>();

        /*
         * Pre: <ListaFicheros> es no vacía
         * Post: Espera a que el directorio destino no contenga el archivo <NombreDestino>, una vez eliminado se encarga de ordenar la <ListaArchivos> de la manera deseada,
         *       para posteriormente copiar el primer archivo a las carpetas <stRutaDestino> y <stRutaProcesados>, con un previo cambio de nombre a <NombreDestino> 
         *       y a "NombreDelArchivo + <DescProcesado> + <ExtensionOrigen>". 
         *       Por último elimina el archivo del directorio <stRutaOrigen>
         */
        private static bool CopiarArchivo(List<InformacionFicheros> ListaArchivos, FtpClient ftp, string remoto)
        {
            try
            {
                // Se comprueba que el directorio <stRutaDestino> exista, en caso de que no se envía un mensaje fatal al log
                if (!Directory.Exists(VariablesGlobales.stRutaDestino))
                {

                    VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL La ruta destino no existe " + VariablesGlobales.sLogServiceName + "...");
                    return false;
                }

                // Se comprueba que el directorio <stRutaProcesado> exista, en caso de que no se envía un aviso al log y se intenta crear.
                // En caso de que no se haya podido crear se envia un mensaje fatal al log.
                if (!Directory.Exists(VariablesGlobales.stRutaProcesado))
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING La ruta destino procesados no existe, se procede a crear directorio \"" + VariablesGlobales.stRutaProcesado + "\" " + VariablesGlobales.sLogServiceName + "...");
                    try
                    {
                        Directory.CreateDirectory(VariablesGlobales.stRutaProcesado);
                        VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING Se ha creado el directorio \"" + VariablesGlobales.stRutaProcesado + "\" " + VariablesGlobales.sLogServiceName);

                    }
                    catch (Exception ex)
                    {
                        VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL No se ha podido crear el directorio \"" + VariablesGlobales.stRutaProcesado + "\" " + VariablesGlobales.sLogServiceName + "ERROR: " + ex.Message);
                        return false;
                    }
                }

                // Se espera a que no exista un archivo con nombre <NombreDestino> en el directorio <stRutaDestino>, en el caso de uqe exista esperaremos <RefreshTime> ms antes de volver a comprobarlo
                while (File.Exists(VariablesGlobales.stRutaDestino + VariablesGlobales.NombreDestino + VariablesGlobales.ExtensionOrigen)) { Thread.Sleep(VariablesGlobales.RefreshTime); }

                ListaArchivos.Sort(VariablesGlobales.TipoOrdenacion(VariablesGlobales.InfoOrdenacion));     // Se ordena la lista según el metodo de ordenación<InfoOrdenación>


                

                // Bucle encargado de buscar cual es el primer archivo con extension <ExtensionOrigen>

                int j = 0;
                InformacionFicheros archivo = ListaArchivos[j];
                for (; !archivo.Name.EndsWith(VariablesGlobales.ExtensionOrigen) && ArchivosNoEliminados.Contains(archivo.Name) && (j < ListaArchivos.Count()); j++)
                {
                    archivo = ListaArchivos[j];
                }

                // Si no hay ningun archivo con <ExtensionOrigen>
                if (j == ListaArchivos.Count())
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING: Los archivos del directorio \"" + VariablesGlobales.stRutaOrigen + "\" han sido ya impresos y deben ser eliminados o no tienen la extension deseada " + VariablesGlobales.sLogServiceName);
                    return true;

                }
                
                try
                {
                    // Se descarga el primer archivo del servidor FTP a un directorio provisional
                    ftp.DownloadFile(remoto + archivo.Name, VariablesGlobales.stRutaOrigen + archivo.Name, FtpLocalExists.Overwrite);

                }
                catch (Exception ex) // En caso de no poder descargarlo se envía un mensaje fatal al log
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: No se ha podido descargar el archivo \"" + VariablesGlobales.stRutaOrigen + archivo.Name + "\" del servidor. ERROR: " + ex.Message);
                    return true;
                }

                try
                {
                    // El archivo extraido se copia en el directorio <stRutaDestino> con nombre <NombreDestino>
                    File.Copy(remoto + archivo.Name, VariablesGlobales.stRutaDestino + VariablesGlobales.NombreDestino + VariablesGlobales.ExtensionOrigen);

                    // Se informa que se ha copiado
                    if (VariablesGlobales.bLogDetallado)
                    {
                        VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Se ha copiado el archivo \"" + archivo.Name + "\" en la carpeta \"" + VariablesGlobales.stRutaDestino + "\" " + VariablesGlobales.sLogServiceName + "...");
                    }
                }
                catch (Exception ex)
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING: Error en el copiado del archivo \"" + archivo.Name + "\" en la carpeta \"" + VariablesGlobales.stRutaDestino + "\" " + VariablesGlobales.sLogServiceName + " ERROR: " + ex.Message);
                    return true;
                }
                
                

                // Se elimina la extension del fichero para poder cambiarle el nombre
                string nombreSinExtension = archivo.Name.Replace(VariablesGlobales.ExtensionOrigen, "");


                try
                {
                    // El archivo extraido se copia en el directorio <stRutaProcesado> con nombre "<nombreSinExtension> + <DescProcesado> + <ExtensionOrigen>"
                    File.Copy(remoto + archivo.Name, VariablesGlobales.stRutaProcesado + nombreSinExtension + VariablesGlobales.DescProcesado + VariablesGlobales.ExtensionOrigen, true);

                    // Se informa del copiado
                    if (VariablesGlobales.bLogDetallado)
                    {
                        VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Se ha copiado el archivo \"" + archivo.Name + "\" en la carpeta \"" + VariablesGlobales.stRutaProcesado + "\" " + VariablesGlobales.sLogServiceName + "...");
                    }
                }
                catch (Exception ex)
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING: Error en el copiado del archivo \"" + archivo.Name + "\" en la carpeta \"" + VariablesGlobales.stRutaProcesado + "\" " + VariablesGlobales.sLogServiceName + " ERROR: " + ex.Message);
                    
                }


                try
                {
                    // El archivo extraido se elimina del servidor FTP de la ruta <stRutaOrigen>
                    ftp.DeleteFile(VariablesGlobales.stRutaOrigen + archivo.Name);
                    if (VariablesGlobales.bLogDetallado)
                    {
                        VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Se ha eliminado el archivo \"" + VariablesGlobales.stRutaOrigen + archivo.Name + "\" del servidor.");
                    }


                }
                catch (Exception ex)
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: No se ha podido eliminar el archivo \"" + VariablesGlobales.stRutaOrigen + archivo.Name + "\" del servidor. ERROR: " + ex.Message);
                    ArchivosNoEliminados.Add(archivo.Name);
                    
                }

                
                return true;
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en el copiado de archivos. ERROR: " + ex.Message);
                return false;
            }



        }

        public static void OrigenFTP()
        {
            try
            {

                // Se informa que se ha iniciado el proceso de lectura y copiado de ficheros
                if (VariablesGlobales.bLogDetallado)
                {
                    VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Ejecutando OrigenFTP " + VariablesGlobales.sLogServiceName + "...");
                }

                FtpClient ftp = new FtpClient();

                try
                {
                    // Se crea un ftp con la ip del host, un usuario y su contraseña
                    ftp = new FtpClient(VariablesGlobales.IP, VariablesGlobales.Usuario,VariablesGlobales.Contrasena);
                    ftp.Connect();


                }
                catch (Exception ex)        
                {
                    // No se ha podido crear 
                    VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL No se ha podido conectar al servidor FTP " + VariablesGlobales.sLogServiceName + " ERROR: " + ex.Message);
                    Error = true;
                    return;
                }

                string remoto = "C:\\UNIVERSIDAD\\Practicas\\nuevo\\REMOTO\\";

                // Se crea un directorio temporal para almacenar los archivos que se vayan descargando
                if (!Directory.Exists(remoto))
                {
                    try
                    {
                        Directory.CreateDirectory(remoto);

                    }
                    catch (Exception ex)
                    {
                        // Si no se puede crear se muestra un Warning
                        VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " No existe y no se ha podido crear el directorio \"" + remoto + "\" los archivos se descargaran en la ubicación actual " + VariablesGlobales.sLogServiceName + " ERROR: " + ex.Message);
                        remoto = "";
                    }
                }


                // Se comprueba que el directorio <stRutaOrigen> exista, en caso de que no se envía un mensaje fatal al log
                if (!ftp.DirectoryExists(VariablesGlobales.stRutaOrigen))
                {
                    VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL La ruta origen no existe " + VariablesGlobales.sLogServiceName + "...");
                    Error = true;
                    return;
                }

                for (int i = 0; i < ArchivosNoEliminados.Count(); i++)
                {
                    string nombre = ArchivosNoEliminados[i];
                    ArchivosNoEliminados.Remove(nombre);

                    try
                    {
                        // El archivo extraido se elimina del servidor FTP de la ruta <stRutaOrigen>
                        ftp.DeleteFile(VariablesGlobales.stRutaOrigen + nombre);
                        if (VariablesGlobales.bLogDetallado)
                        {
                            VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Se ha eliminado el archivo \"" + VariablesGlobales.stRutaOrigen + nombre + "\" del servidor.");
                        }


                    }
                    catch (Exception ex)
                    {
                        VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: No se ha podido eliminar el archivo \"" + VariablesGlobales.stRutaOrigen + nombre + "\" del servidor. ERROR: " + ex.Message);
                        ArchivosNoEliminados.Add(nombre);
                        
                    }
                }

                List<InformacionFicheros> ListaArchivos = new List<InformacionFicheros>();

                // Se almacena la información de todos los archivos del directorio en una lista
                foreach (FtpListItem item in ftp.GetListing(VariablesGlobales.stRutaOrigen))
                {
                    if (item.Type == FtpObjectType.File)
                    {
                        InformacionFicheros fichero = new InformacionFicheros();

                        fichero.Name = item.Name;
                        fichero.CreationTime = item.Created.ToString();
                        fichero.LastWriteTime = item.Modified.ToString();

                        ListaArchivos.Add(fichero);
                    }

                }

                // Antes de comenzar el proceso de copia se asegura de que haya archivos en el directorio
                if (ListaArchivos.Count > 0)
                {
                    if(!CopiarArchivo(ListaArchivos, ftp, remoto))
                    {
                        Error = true;
                        return;
                    }
                }

                ftp.Disconnect();
            
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en OrigenFTP. ERROR: " + ex.Message);
                Error = true;
                return;
            }
            
        }
    }
}
