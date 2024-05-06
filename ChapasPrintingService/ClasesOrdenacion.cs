using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapasPrintingService
{
    /*
    * Clase base abstracta de formas de ordenación. De ella derivan el resto de clases especifícas para cada ordenación.
    * Utiliza una sobrecarga de métodos para poder comparar dos clases diferentes
    */
    public abstract class TiposDeOrdenaciones : IComparer<FileInfo>, IComparer<InformacionFicheros>
    {
        public abstract int Compare(FileInfo a, FileInfo b);

        public abstract int Compare(InformacionFicheros a, InformacionFicheros b);

    }
    /*
     * Clase ordenación alfabética ascendente
     */
    public class OrdenacionAlfAsc : TiposDeOrdenaciones
    {
        public override int Compare(FileInfo a, FileInfo b)
        {
            return a.Name.CompareTo(b.Name);
        }

        public override int Compare(InformacionFicheros a, InformacionFicheros b)
        {
            return a.Name.CompareTo(b.Name);
        }
    }
    /*
     * Clase ordenación alfabética descendente
     */
    public class OrdenacionAlfDesc : TiposDeOrdenaciones
    {
        public override int Compare(FileInfo a, FileInfo b)
        {
            return b.Name.CompareTo(a.Name);
        }
        public override int Compare(InformacionFicheros a, InformacionFicheros b)
        {
            return b.Name.CompareTo(a.Name);

        }
    }
    /*
     * Clase ordenación creación ascendente
     */
    public class OrdenacionCreacionAsc : TiposDeOrdenaciones
    {
        public override int Compare(FileInfo a, FileInfo b)
        {
            return a.CreationTime.CompareTo(b.CreationTime);
        }
        public override int Compare(InformacionFicheros a, InformacionFicheros b)
        {
            return a.CreationTime.CompareTo(b.CreationTime);
        }
        /*
     * Clase ordenación creación descendente
     */
    }
    public class OrdenacionCreacionDesc : TiposDeOrdenaciones
    {
        public override int Compare(FileInfo a, FileInfo b)
        {
            return b.CreationTime.CompareTo(a.CreationTime);
        }
        public override int Compare(InformacionFicheros a, InformacionFicheros b)
        {
            return b.CreationTime.CompareTo(a.CreationTime);
        }
    }
    /*
     * Clase rdenación última modficación ascendente
     */
    public class OrdenacionModificacionAsc : TiposDeOrdenaciones
    {
        public override int Compare(FileInfo a, FileInfo b)
        {
            return a.LastWriteTime.CompareTo(b.LastWriteTime);
        }
        public override int Compare(InformacionFicheros a, InformacionFicheros b)
        {
            return a.LastWriteTime.CompareTo(b.LastWriteTime);
        }
    }
    /*
     * Clase ordenación ultima modifcación descente
     */
    public class OrdenacionModificacionDesc : TiposDeOrdenaciones
    {
        public override int Compare(FileInfo a, FileInfo b)
        {
            return b.LastWriteTime.CompareTo(a.LastWriteTime);
        }
        public override int Compare(InformacionFicheros a, InformacionFicheros b)
        {
            return b.LastWriteTime.CompareTo(a.LastWriteTime);
        }
    }
    /*
     * Clase para almacenar información del fichero del servidor FTP. Su metodo de comporación por defecto es por el nombre de forma alfabéticamente ascendente
     */
    public class InformacionFicheros : IComparable<InformacionFicheros>
    {

        public string Name;
        public string CreationTime;
        public string LastWriteTime;

        public int CompareTo(InformacionFicheros other)
        {
            return this.Name.CompareTo(other.Name);
        }
    }
}

