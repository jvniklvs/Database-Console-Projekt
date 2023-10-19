using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;


namespace BibliothekDatenbank
{
    public class Bibliothek : DataContext 
    {
        // Table<T> abstracts database details per table/data type.
        public Table<Bücherlist> Bücherliste; 

        public Bibliothek(string connection) : base(connection) { }  
    }

    [Table(Name = "Bücherliste")]
    public class Bücherlist
    {
        private string _ISBN;
        [Column(Storage ="_ISBN", DbType ="VARCHAR(20)", IsPrimaryKey = true)]
        public string ISBN
        {
            get { return this._ISBN; }
            set { this._ISBN = value; }
        }

        private string _Buchname;
        [Column(Storage ="_Buchname")]
        public string Buchname
        {
            get { return this._Buchname; }
            set { this._Buchname = value; }
        }

        private decimal _Preis;
        [Column(Storage ="_Preis")]
        public decimal Preis
        {
            get { return this._Preis; }
            set { this._Preis = value; }
        }
    }
}
