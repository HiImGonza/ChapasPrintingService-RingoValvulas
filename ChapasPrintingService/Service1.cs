
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Rules;
using System.Net.NetworkInformation;
using System.Web.UI.WebControls;


namespace ChapasPrintingService
{ 
    public partial class Service1 : ServiceBase
    {

        bool ProcesoActivo = false;          // Variable que impide que se ejecuten mas de un thread a la vez debido a varios cambios simultaneos en el directorio origen

        public Service1()
        {
            InitializeComponent();
        }      
        /*
         * Funcion que se ejecuta al iniciar el servicio.
         */
        protected override void OnStart(string[] args)
        {
            try
            {

                ProcesoActivo = true;

                if (VariablesGlobales.bLogDetallado)
                {
                    VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Ejecutando OnStart " + VariablesGlobales.sLogServiceName + "...");
                }


                Thread mensajero;

                // Lanzamos la primera ejecución, que generará una reacción en cadena
                if (VariablesGlobales.TipoOrigen.Equals("DIR"))
                {
                    try
                    {
                        visorFichero.Path = VariablesGlobales.stRutaOrigen;             // Añadimos la ruta del directorio que estará vigilando

                    }
                    catch (Exception ex)
                    {
                        VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: La ruta del directorio origen \"" + VariablesGlobales.stRutaOrigen + "\" no existe ERROR: " + ex.Message);

                    }
                    visorFichero.Filter = "*" + VariablesGlobales.ExtensionOrigen;  // El visor solo reaccionará a cambios en la carpeta con la extensión dada
                    mensajero = new Thread(new ThreadStart(Local.OrigenLocal));
                }
                else // if (VariablesGlobales.TipoOrigen.Equals("FTP"))
                {
                    Temporizador.Interval = VariablesGlobales.RefreshTime;
                    Temporizador.Start();
                    mensajero = new Thread(new ThreadStart(FTP.OrigenFTP));

                }

                mensajero.Start();
                mensajero.Join();
                ProcesoActivo = false;
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en el proceso OnStart. ERROR: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (VariablesGlobales.bLogDetallado)
                {
                    VariablesGlobales.Log.Info(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " INFO Ejecutando OnStop " + VariablesGlobales.sLogServiceName + "...");
                }
                if (VariablesGlobales.TipoOrigen.Equals("FTP"))
                {
                    Temporizador.Stop();
                    if (Directory.Exists("C:\\UNIVERSIDAD\\Practicas\\nuevo\\REMOTO\\"))
                    {
                        Directory.Delete("C:\\UNIVERSIDAD\\Practicas\\nuevo\\REMOTO\\", true);
                    }
                }
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en el proceso OnStop. ERROR: " + ex.Message);

            }

        }

        private void visorFichero_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Local.Error)
                {
                    visorFichero.EnableRaisingEvents = false;
                } 
                Thread.Sleep(10); // Pausa para darle tiempo a los archivos a copiarse
                if (ProcesoActivo) { return; }
                ProcesoActivo = true;

                Thread mensajero;
                mensajero = new Thread(new ThreadStart(Local.OrigenLocal));


                mensajero.Start();
                mensajero.Join();
                ProcesoActivo = false;
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en el proceso Observador. ERROR: " + ex.Message);
            }


        }

        private void Temporizador_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {   
            try
            {
                if (VariablesGlobales.TipoOrigen.Equals("FTP"))
                {
                    if (FTP.Error)
                    {
                        Temporizador.Enabled = false;
                    }

                    if (ProcesoActivo) { return; }

                    ProcesoActivo = true;

                    Thread mensajero;
                    mensajero = new Thread(new ThreadStart(FTP.OrigenFTP));


                    mensajero.Start();
                    mensajero.Join();

                    ProcesoActivo = false;
                }
            }
            catch (Exception ex)
            {
                VariablesGlobales.Log.Fatal(DateTime.Now.ToString() + " SERVICIO: " + VariablesGlobales.sLogServiceName + " FATAL: Ha ocurrido un error inesperado en el proceso Temporizador. ERROR: " + ex.Message);
            }
            
        }
    }
}
