using System;
using System.Xml.Linq;

namespace Sklep
{
    class Program
    {
        class Klient
        {
            private string email;
            private string haslo;

            public Klient(string email, string haslo)
            {
                this.email = email;
                this.haslo = haslo;
            }
        }

        class Lista_klientow
        {
            private List<Klient> klienci;

            public Lista_klientow()
            {
                klienci = new List<Klient>();
            }
        }

        class Produkt
        {
            public string Nazwa { get; }
            private double liczba_sztuk;
            public double Liczba_sztuk
            {
                get { return liczba_sztuk; }
                set
                {
                    if (Liczba_sztuk - value > 0)
                    {
                        Liczba_sztuk = (uint)((int)Liczba_sztuk + value);
                    }
                    else
                    {
                        Liczba_sztuk = 0;
                    }
                }
            }
            private double cena { get; }

            public Produkt(string nazwa, uint liczba_sztuk)
            {
                this.Nazwa = nazwa;
                this.Liczba_sztuk = liczba_sztuk;
            }

            static void Main(string[] args)
            {



            }
        }
    }
}