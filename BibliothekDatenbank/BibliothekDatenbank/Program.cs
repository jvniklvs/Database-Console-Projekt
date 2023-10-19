using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace BibliothekDatenbank
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string connectionString = ConfigurationManager.ConnectionStrings["BücherlisteConnection"].ConnectionString; //Abruf des Connectionstrings aus der App.config
            Bibliothek db = new Bibliothek(connectionString); //Erstellung einer Instanz
            XElement booksXml = XElement.Load("Bücherliste.xml"); //Die XML Datei wird geladen

            Console.WriteLine(connectionString); //Ausgabe des Connectionstrings

            while (true)
            {
                Console.WriteLine("\n1. Daten aus XML-Datei lesen und bearbeiten");
                Console.WriteLine("2. Neues Buch hinzufügen");
                Console.WriteLine("3. Buch löschen");
                Console.WriteLine("4. Synchronisieren der XML-Datei mit der Datenbank");
                Console.WriteLine("5. Synchronisieren der Datenbank mit der XML-Datei");
                Console.WriteLine("6. Programm schließen");
                Console.WriteLine();
                string _choice = Console.ReadLine(); //Konvertiert die Eingabe in einen Integer

                if (IsNumber(_choice))      //Beim nächsten Mal vielleicht Switch??!
                {
                    int choice = Convert.ToInt32(_choice);

                    if (choice == 1)
                    {
                        ReadAndModifyData(booksXml);
                    }
                    else if (choice == 2)
                    {
                        AddNewBook(booksXml);
                    }
                    else if (choice == 3)
                    {
                        DeleteBook(booksXml);
                    }
                    else if (choice == 4)
                    {
                        UpdateXmlFromDatabase(booksXml, db);
                    }
                    else if (choice == 5)
                    {
                        UpdateDatabaseFromXml(booksXml, db);
                    }
                    else if (choice == 6)
                    {
                        Environment.Exit(0); //Schließt die Konsole
                    }
                    else
                    {
                        Console.WriteLine("Ungültige Eingabe. Bitte wählen Sie 1, 2, 3 oder 4.");
                    }
                }
                else
                {
                    Console.WriteLine("Der eingegebene Parameter entspricht keiner Zahl");
                    Console.WriteLine("");
                }
            }
        }
        static void SaveDataToXml(XElement booksXml)
        {
            booksXml.Save("Bücherliste.xml"); //speichert die Daten in der XML Liste
            Console.WriteLine("\nDaten wurden in Bücherliste.xml gespeichert.");
        }

        static void ReadAndModifyData(XElement booksXml)
        {
            var bookQuery =                                     //LINQ Abfrage, was abgefragt werden soll
                from book in booksXml.Elements("Buch")
                select new
                {
                    ISBN = book.Element("ISBN").Value,
                    Buchname = book.Element("Buchname").Value,
                    Preis = book.Element("Preis").Value,
                };

            foreach (var book in bookQuery) //Ausgabe der Abgefragten Elemente
            {
                Console.WriteLine($"\nISBN: {book.ISBN} | Buchname: {book.Buchname} | Preis: {book.Preis}\u20AC");
            }

            Console.WriteLine("\nWollen Sie den Preis eines Buches bearbeiten? Geben Sie die ISBN des Buches ein (oder 'q' zum Beenden):");

            string isbnToEdit = Console.ReadLine();

            if (isbnToEdit.ToLower() == "q")
            {
                return; //Wenn q eingegeben wurde, dann verlässt man die Funktion
            }

            var bookToEdit = bookQuery.FirstOrDefault(book => book.ISBN == isbnToEdit); //Überpüfung, ob ein Element existiert, welches mit dieser ISBN übereinstimmt

            if (bookToEdit != null) //Überprüfung ob das Element null ist
            {
                Console.Write("Geben Sie den neuen Preis ein: ");
                string newPreis = Console.ReadLine();

                XElement bookElement = booksXml.Elements("Buch")
                .Single(b => b.Element("ISBN").Value == bookToEdit.ISBN); //findet das Element, welches bearbeitet werden soll

                bookElement.Element("Preis").Value = newPreis; //neuer Preis wird aktualisiert 

                Console.WriteLine($"Preis für ISBN {bookToEdit.ISBN} wurde aktualisiert.");

            }
            else
            {
                Console.WriteLine("ISBN nicht gefunden.");
            }

            SaveDataToXml(booksXml);

            ReadAndModifyData(booksXml); //erneute Ausführung, um nicht direkt wieder ins "Menü" zu gelangen
        }

        static void AddNewBook(XElement booksXml) //statische Methode ohne Rückgabewert
        {
            Console.WriteLine("\nNeues Buch hinzufügen: ");
            Console.WriteLine("Drücke 'Enter' um ein neues Buch hinzuzufügen oder 'q' zum Beenden");
            string leaveOrStay = Console.ReadLine();
            if (leaveOrStay == "q")
            {
                return;
            }

            Console.Write("ISBN: ");
            string isbn = Console.ReadLine();
            Console.Write("Buchname: ");
            string buchname = Console.ReadLine();
            Console.Write("Preis: ");
            decimal preis = Convert.ToDecimal(Console.ReadLine());


            XElement newBook = new XElement("Buch",     //Erstellung eines Elements, das nach dem richtigen Schema aufgebaut ist 
                new XElement("ISBN", isbn),
                new XElement("Buchname", buchname),
                new XElement("Preis", preis)
                );

            booksXml.Add(newBook); //Das neue Element wird der xml Datei hinugefügt

            Console.WriteLine("Neues Buch wurde hinzugefügt.");

            SaveDataToXml(booksXml);

            AddNewBook(booksXml);

        }

        static void DeleteBook(XElement booksXml)
        {
            Console.WriteLine("\nBuch löschen:");
            Console.Write("ISBN des zu löschenden Buchs (oder 'q' zum Beenden): ");
            string isbnToDelete = Console.ReadLine();

            if (isbnToDelete.ToLower() == "q")
            {
                return;
            }

            XElement bookToDelete = booksXml.Elements("Buch").FirstOrDefault(book => book.Element("ISBN").Value == isbnToDelete); //Es wird jedes Element überprüft, ob es mit isbnToDelete übereinstimmt und das erste, wodrauf das zutrifft, wird ausgewählt

            if (bookToDelete != null)
            {
                bookToDelete.Remove(); //das ausgewählte Element wird gelöscht 
                Console.WriteLine("Buch wurde gelöscht.");
            }
            else
            {
                Console.WriteLine("Die angegebene ISBN wurde nicht gefunden.");
            }

            SaveDataToXml(booksXml);

            DeleteBook(booksXml); //erneute Ausführung, um nicht direkt wieder ins "Menü" zu gelangen
        }

        static void UpdateDatabaseFromXml(XElement booksXml, Bibliothek db)
        {

                // Löscht alle vorhandenen Datensätze in der Datenbanktabelle.
                db.Bücherliste.DeleteAllOnSubmit(db.Bücherliste);

                // Fügt die Datensätze aus der XML-Datei in die Datenbank ein.
                foreach (var book in booksXml.Elements("Buch"))
                {
                    db.Bücherliste.InsertOnSubmit(new Bücherlist
                    {
                        ISBN = book.Element("ISBN").Value,
                        Buchname = book.Element("Buchname").Value,
                        Preis = Convert.ToDecimal(book.Element("Preis").Value)
                    });
                }

                db.SubmitChanges(); //Bestätigt die Änderungen und führt diese aus

                Console.WriteLine("Daten in der Datenbank wurden aktualisiert.");
        }

        static void UpdateXmlFromDatabase(XElement booksXml, Bibliothek db)
        {
            // Entfernt alle vorhandenen Buch-Elemente aus der XML-Datei.
            booksXml.Elements("Buch").Remove();

            // Fügt die Datensätze aus der Datenbank in die XML-Datei ein.
            foreach (var book in db.Bücherliste)
            {
                booksXml.Add(new XElement("Buch",
                    new XElement("ISBN", book.ISBN),
                    new XElement("Buchname", book.Buchname),
                    new XElement("Preis", book.Preis.ToString("f2"))
                ));
            }

            SaveDataToXml(booksXml);

            Console.WriteLine("\nDaten in der XML-Datei wurden aktualisiert.");
        }
        static bool IsNumber(string choice)
        {
            if (int.TryParse(choice, out _)) //Überprüft, ob man choice in einen Integer umwandeln kann (Rückgabewert ist ein bool)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
