using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ChapasPrintingService
{
    internal class Local
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
        private static bool CopiarArchivo(List<FileInfo> ListaArchivos)
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

                ListaArchivos.Sort(VariablesGlobales.TipoOrdenacion(VariablesGlobales.InfoOrdenacion));    // Se ordena la lista según el metodo de ordenación <InfoOrdenación>

                int j = 0;
                FileInfo archivo = ListaArchivos[j];
                for(; ArchivosNoEliminados.Contains(archivo.FullName) && (j < ListaArchivos.Count()); j++)
                {
                    archivo = ListaArchivos[j];
                }

                
                
                if(j == ListaArchivos.Count())
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING: Todos los archivos del directorio \"" + VariablesGlobales.stRutaOrigen + "\" han sido ya impresos y deben ser eliminados " + VariablesGlobales.sLogServiceName);
                    return true;
                }
                try
                {
                    // El archivo extraido se copia en el directorio <stRutaDestino> con nombre <NombreDestino>
                    File.Copy(archivo.FullName, VariablesGlobales.stRutaDestino + VariablesGlobales.NombreDestino + VariablesGlobales.ExtensionOrigen);

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
                    File.Copy(archivo.FullName, VariablesGlobales.stRutaProcesado + nombreSinExtension + VariablesGlobales.DescProcesado + VariablesGlobales.ExtensionOrigen, true);

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
                    File.Delete(archivo.FullName);
                    if (VariablesGlobales.bLogDetallado)
                    {
                        VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Se ha eliminado el archivo \"" + archivo.Name + "\" de la carpeta \"" + VariablesGlobales.stRutaOrigen + "\" " + VariablesGlobales.sLogServiceName + "...");
                    }


                }
                catch (Exception ex)
                {
                    VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " WARNING: No se ha podido eliminar el archivo \"" + archivo.Name + "\" de la carpeta \"" + VariablesGlobales.stRutaOrigen + "\" " + VariablesGlobales.sLogServiceName + " ERROR: " + ex.Message);
                    ArchivosNoEliminados.Add(archivo.FullName);
                    
                }


                return true;
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en el copiado de archivos. ERROR: " + ex.Message);
                return false;

            }


        }
        /*
         * Metodo encargado de leer de un directorio origen todos los archivos con <ExtensionOrigen>, para posteriormente copiar el primero de ellos a otra carpeta
         */
        public static void OrigenLocal()
        {
            try
            {
                // Se informa que se ha iniciado el proceso de lectura y copiado de ficheros
                if (VariablesGlobales.bLogDetallado)        
                {
                    VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Ejecutando OrigenLocal " + VariablesGlobales.sLogServiceName + "...");
                }

                // Se comprueba que el directorio <stRutaOrigen> exista, en caso de que no se envía un mensaje fatal al log
                if (!Directory.Exists(VariablesGlobales.stRutaOrigen))
                {
                    VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL La ruta origen no existe " + VariablesGlobales.sLogServiceName + "...");
                    Error = true;
                    return;
                }

                // Intentamos volver a eliminar los archivos no eliminados
                for (int i = 0; i < ArchivosNoEliminados.Count(); i++)
                {
                    string nombre = ArchivosNoEliminados[i];
                    ArchivosNoEliminados.Remove(nombre);

                    try
                    {
                        // El archivo extraido se elimina del servidor FTP de la ruta <stRutaOrigen>
                        File.Delete(nombre);
                        if (VariablesGlobales.bLogDetallado)
                        {
                            VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Se ha eliminado el archivo \"" + nombre + "\" de la carpeta \"" + VariablesGlobales.stRutaOrigen + "\" " + VariablesGlobales.sLogServiceName + "...");
                        }

                    }
                    catch (Exception ex)
                    {

                        VariablesGlobales.Log.Warn(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: No se ha podido eliminar el archivo \"" + nombre + "\" de la carpeta \"" + VariablesGlobales.stRutaOrigen + "\" " + VariablesGlobales.sLogServiceName + " ERROR: " + ex.Message);
                        ArchivosNoEliminados.Add(nombre);
                    }
                }

                DirectoryInfo directorio = new DirectoryInfo(VariablesGlobales.stRutaOrigen);   // Se obtiene toda la información del directorio <stRutaOrigen>

                
                List<FileInfo> ListaArchivos = directorio.GetFiles("*" + VariablesGlobales.ExtensionOrigen).ToList<FileInfo>(); // Se almacena la información de sus archivos con <ExtensionOrigen> en una lista

                // Antes de comenzar el proceso de copia se asegura de que haya archivos en el directorio
                if (ListaArchivos.Count != 0)
                {
                    if (!CopiarArchivo(ListaArchivos))
                    {
                        Error = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en OrigenLocal. ERROR: " + ex.Message);
                    Error = true;
                return;
            }
            
        }
    }
}
