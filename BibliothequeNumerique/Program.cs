using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BibliothequeNumerique
{
    class DocumentNonTrouveException : Exception
    {
        public DocumentNonTrouveException(string message) : base(message) { }
    }

    abstract class Document
    {
        public Guid Id { get; set; }
        public string Titre { get; set; }
        public string Auteur { get; set; }
        public int Annee { get; set; }

        public Document(string titre, string auteur, int annee)
        {
            Id = Guid.NewGuid();
            Titre = titre;
            Auteur = auteur;
            Annee = annee;
        }

        public abstract void AfficherDetails();
        public abstract string ToCsv();
    }

    class Livre : Document
    {
        public int NombrePages { get; set; }

        public Livre(string titre, string auteur, int annee, int pages)
            : base(titre, auteur, annee)
        {
            NombrePages = pages;
        }

        public override void AfficherDetails()
        {
            Console.WriteLine($"[Livre] {Titre} - {Auteur} ({Annee}) | Pages: {NombrePages}");
        }

        public override string ToCsv()
        {
            return $"Livre;{Titre};{Auteur};{Annee};{NombrePages}";
        }
    }

    class Magazine : Document
    {
        public int Numero { get; set; }

        public Magazine(string titre, string auteur, int annee, int numero)
            : base(titre, auteur, annee)
        {
            Numero = numero;
        }

        public override void AfficherDetails()
        {
            Console.WriteLine($"[Magazine] {Titre} - {Auteur} ({Annee}) | Numéro: {Numero}");
        }

        public override string ToCsv()
        {
            return $"Magazine;{Titre};{Auteur};{Annee};{Numero}";
        }
    }

    class DocumentPDF : Document
    {
        public double TailleEnMo { get; set; }

        public DocumentPDF(string titre, string auteur, int annee, double taille)
            : base(titre, auteur, annee)
        {
            TailleEnMo = taille;
        }

        public override void AfficherDetails()
        {
            Console.WriteLine($"[PDF] {Titre} - {Auteur} ({Annee}) | Taille: {TailleEnMo} Mo");
        }

        public override string ToCsv()
        {
            return $"PDF;{Titre};{Auteur};{Annee};{TailleEnMo}";
        }
    }

    class Bibliotheque
    {
        private List<Document> documents = new List<Document>();

        public void AjouterDocument(Document d)
        {
            documents.Add(d);
            Console.WriteLine("✅ Document ajouté.");
        }

        public void SupprimerDocument(Guid id)
        {
            var doc = documents.FirstOrDefault(d => d.Id == id);
            if (doc == null)
                throw new DocumentNonTrouveException("❌ Document introuvable.");

            documents.Remove(doc);
            Console.WriteLine("✅ Document supprimé.");
        }

        public void Rechercher(string motCle)
        {
            var resultats = documents
                .Where(d => d.Titre.Contains(motCle, StringComparison.OrdinalIgnoreCase)).ToList();

            if (resultats.Count == 0)
                throw new DocumentNonTrouveException("❌ Aucun document trouvé.");

            foreach (var d in resultats)
                d.AfficherDetails();
        }

        public void AfficherTous()
        {
            if (documents.Count == 0)
            {
                Console.WriteLine("Bibliothèque vide.");
                return;
            }

            foreach (var d in documents)
                d.AfficherDetails();
        }

        public void Sauvegarder(string chemin)
        {
            using (FileStream fs = new FileStream(chemin, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (var d in documents)
                    sw.WriteLine(d.ToCsv());
            }

            Console.WriteLine("✅ Sauvegarde réussie.");
        }

        public void Charger(string chemin)
        {
            if (!File.Exists(chemin))
                throw new FileNotFoundException("❌ Fichier introuvable.");

            documents.Clear();

            using (FileStream fs = new FileStream(chemin, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            {
                string ligne;
                while ((ligne = sr.ReadLine()) != null)
                {
                    var parts = ligne.Split(';');
                    string type = parts[0];
                    string titre = parts[1];
                    string auteur = parts[2];
                    int annee = int.Parse(parts[3]);

                    if (type == "Livre")
                        documents.Add(new Livre(titre, auteur, annee, int.Parse(parts[4])));
                    else if (type == "Magazine")
                        documents.Add(new Magazine(titre, auteur, annee, int.Parse(parts[4])));
                    else if (type == "PDF")
                        documents.Add(new DocumentPDF(titre, auteur, annee, double.Parse(parts[4])));
                    else
                        throw new Exception("Type inconnu.");
                }
            }

            Console.WriteLine("✅ Chargement réussi.");
        }
    }

    class Program
    {
        static void Main()
        {
            Bibliotheque biblio = new Bibliotheque();

            while (true)
            {
                Console.WriteLine("\n1. Ajouter un document");
                Console.WriteLine("2. Afficher tous");
                Console.WriteLine("3. Rechercher");
                Console.WriteLine("4. Supprimer");
                Console.WriteLine("5. Sauvegarder");
                Console.WriteLine("6. Charger");
                Console.WriteLine("7. Quitter");
                Console.Write("Choix : ");

                try
                {
                    string choix = Console.ReadLine();

                    if (choix == "1")
                    {
                        Console.Write("Type (Livre/Magazine/PDF) : ");
                        string type = Console.ReadLine();

                        Console.Write("Titre : ");
                        string titre = Console.ReadLine();
                        Console.Write("Auteur : ");
                        string auteur = Console.ReadLine();
                        Console.Write("Année : ");
                        int annee = int.Parse(Console.ReadLine());

                        if (type == "Livre")
                        {
                            Console.Write("Pages : ");
                            int pages = int.Parse(Console.ReadLine());
                            biblio.AjouterDocument(new Livre(titre, auteur, annee, pages));
                        }
                        else if (type == "Magazine")
                        {
                            Console.Write("Numéro : ");
                            int num = int.Parse(Console.ReadLine());
                            biblio.AjouterDocument(new Magazine(titre, auteur, annee, num));
                        }
                        else if (type == "PDF")
                        {
                            Console.Write("Taille Mo : ");
                            double taille = double.Parse(Console.ReadLine());
                            biblio.AjouterDocument(new DocumentPDF(titre, auteur, annee, taille));
                        }
                    }
                    else if (choix == "2") biblio.AfficherTous();
                    else if (choix == "3")
                    {
                        Console.Write("Mot-clé : ");
                        biblio.Rechercher(Console.ReadLine());
                    }
                    else if (choix == "4")
                    {
                        Console.Write("ID : ");
                        biblio.SupprimerDocument(Guid.Parse(Console.ReadLine()));
                    }
                    else if (choix == "5") biblio.Sauvegarder("bibliotheque.txt");
                    else if (choix == "6") biblio.Charger("bibliotheque.txt");
                    else if (choix == "7") break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
