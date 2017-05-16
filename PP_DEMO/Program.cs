using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MPI;

namespace PP_DEMO
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                //Zahtjevi --> tag 0
                //Slanje vilica --> tag 1
                Intracommunicator comm = Communicator.world;
                Random rand = new Random();
                Vilica lijeva, desna;         //Vilice
                string poruka;
                bool imaliodgovoralijevo = false;   //Svi moguci odgovori
                bool imaliodgovoradesno = false;
                bool imalizahtjevalijevo = false;
                bool imalizahtjevadesno = false;

                //Numeracije susjeda
                int lijevisusjed = (comm.Rank + 1) % comm.Size;
                int desnisusjed = (comm.Rank - 1);
                if (desnisusjed == -1) desnisusjed = comm.Size - 1;
                
                //Slucaj nultog procesa
                if (comm.Rank == 0)
                {
                    lijeva = new Vilica(true);
                    desna = new Vilica(true);
                    
                } //Slucaj ne-zadnjeg procesa
                else if (comm.Rank != comm.Size - 1)
                {
                    lijeva = new Vilica(true);
                    desna = new Vilica(false);
                }
                else //Slucaj zadnjeg procesa
                {
                    lijeva = new Vilica(false);
                    desna = new Vilica(false);
                }

                //Ponavljaj zauvijek
                while (true)
                {
                    //Misli
                    int random = rand.Next(10);
                    for (int i = 1; i <= comm.Rank; i++)
                    {
                        Console.Write("\t\t\t");
                    }
                    Console.WriteLine(comm.Rank+": Mislim");

                    while (random-- > 0)
                    {
                        //... i 'istovremeno odgovaraj na zahtjeve! (tag = 0)
                        imalizahtjevalijevo = imaliporuka(comm, lijevisusjed, 0);
                        imalizahtjevadesno = imaliporuka(comm, desnisusjed, 0);
                        if (imalizahtjevalijevo)
                        {
                            poruka = comm.Receive<string>(lijevisusjed, 0);
                            //Ako je prljava, ocisti i posalji (tag = 1)
                            if (lijeva.Prljava) comm.Send("Cista vilica", lijevisusjed, 1);
                            for (int i = 1; i <= comm.Rank; i++)
                            {
                                Console.Write("\t\t\t");
                            }
                            Console.WriteLine(comm.Rank + ": Predajem vilicu: "+lijevisusjed);
                            lijeva.Predaj();
                        }
                        if (imalizahtjevadesno)
                        { 
                            poruka = comm.Receive<string>(desnisusjed, 0);
                            //Ako je prljava, ocisti i posalji (tag = 1)
                            if (desna.Prljava) comm.Send("Cista vilica", desnisusjed, 1);
                            for (int i = 1; i <= comm.Rank; i++)
                            {
                                Console.Write("\t\t\t");
                            }
                            Console.WriteLine(comm.Rank + ": Predajem vilicu: "+desnisusjed);
                            desna.Predaj();
                        }
                        Thread.Sleep(1000);
                    }

                    //Trazi vilice
                    while (lijeva.Trebam && desna.Trebam)
                    {
                        //Posalji zahtjeve za vilice (tag = 0)
                        if (lijeva.Trebam)
                        {
                            comm.Send("Trebam vilicu", lijevisusjed, 0);
                            for (int i = 1; i <= comm.Rank; i++)
                            {
                                Console.Write("\t\t\t");
                            }
                            Console.WriteLine(comm.Rank+": Trazim vilicu od: "+lijevisusjed);
                        }
                    
                        if (desna.Trebam)
                        {
                            comm.Send("Trebam vilicu", desnisusjed, 0);
                            for (int i = 1; i <= comm.Rank; i++)
                            {
                                Console.Write("\t\t\t");
                            }
                            Console.WriteLine(comm.Rank+": Trazim vilicu od: "+desnisusjed);
                        }

                        //Dok god ne dobijes vilice ponavljaj::::::::::::::::::::::::::

                        //Provjeri ima li poruka opcenito
                        imaliodgovoralijevo = imaliporuka(comm, lijevisusjed, 1);
                        imaliodgovoradesno = imaliporuka(comm, desnisusjed, 1);
                        imalizahtjevalijevo = imaliporuka(comm, lijevisusjed, 0);
                        imalizahtjevadesno = imaliporuka(comm, desnisusjed, 0);

                        //Ako je poruka odgovor na zahtjev
                        if (imaliodgovoralijevo || imaliodgovoradesno)
                        {
                            if (imaliodgovoralijevo)
                            {
                                //Azuriraj lijevu cistu vilicu (primi ju) tag = 1
                                poruka = comm.Receive<string>(lijevisusjed, 1);
                                for (int i = 1; i <= comm.Rank; i++)
                                {
                                    Console.Write("\t\t\t");
                                }
                                Console.WriteLine(comm.Rank + ": Uzimam vilicu od: "+lijevisusjed);
                                lijeva.Uzmi();
                                lijeva.Ocisti();
                            }
                            if (imaliodgovoradesno)
                            {
                                //Azuriraj desnu cistu vilicu (primi ju) tag = 1
                                poruka = comm.Receive<string>(desnisusjed, 1);
                                for (int i = 1; i <= comm.Rank; i++)
                                {
                                    Console.Write("\t\t\t");
                                }
                                Console.WriteLine(comm.Rank + ": Uzimam vilicu od: "+desnisusjed);
                                desna.Uzmi();
                                desna.Ocisti();
                            }
                            if (lijeva.Imam && desna.Imam) break;
                        } else if (imalizahtjevalijevo || imalizahtjevadesno)
                        {
                            //Ako je vilica slucajno prljava i nalazi se kod tebe, iako si gladan po pravilima moras poslati :P
                            if (imalizahtjevalijevo)
                            {
                                poruka = comm.Receive<string>(lijevisusjed, 0);
                                //Ako je prljava, ocisti i posalji (tag = 1)
                                if (lijeva.Prljava) comm.Send("Cista vilica", lijevisusjed, 1);
                                for (int i = 1; i <= comm.Rank; i++)
                                {
                                    Console.Write("\t\t\t");
                                }
                                Console.WriteLine(comm.Rank + ": Predajem vilicu: "+lijevisusjed);
                                lijeva.Predaj();
                            }
                            if (imalizahtjevadesno)
                            {
                                poruka = comm.Receive<string>(desnisusjed, 0);
                                //Ako je prljava, ocisti i posalji (tag = 1)
                                if (desna.Prljava) comm.Send("Cista vilica", desnisusjed, 1);
                                for (int i = 1; i <= comm.Rank; i++)
                                {
                                    Console.Write("\t\t\t");
                                }
                                Console.WriteLine(comm.Rank + ": Predajem vilicu: "+desnisusjed);
                                desna.Predaj();
                            }
                        }
                        Thread.Sleep(1000);
                    }

                    //Jedi
                    lijeva.Zaprljaj();
                    desna.Zaprljaj();
                    random = rand.Next(20);
                    for (int i = 1; i <= comm.Rank; i++)
                    {
                        Console.Write("\t\t\t");
                    }
                    Console.WriteLine(comm.Rank+": Jedem");
                    Thread.Sleep(random);

                    //Odgovori na postojece zahtjeve (tag = 0)
                    imalizahtjevalijevo = imaliporuka(comm, lijevisusjed, 0);
                    imalizahtjevadesno = imaliporuka(comm, desnisusjed, 0);
                    if (imalizahtjevalijevo)
                    {
                        poruka = comm.Receive<string>(lijevisusjed, 0);
                        //Ako je prljava, ocisti i posalji
                        if (lijeva.Prljava) comm.Send("Cista vilica", lijevisusjed, 1);
                        lijeva.Predaj();
                    }
                    if (imalizahtjevadesno)
                    {
                        poruka = comm.Receive<string>(desnisusjed, 0);
                        //Ako je prljava, ocisti i posalji
                        if (desna.Prljava) comm.Send("Cista vilica", desnisusjed, 1);
                        desna.Predaj();
                    }
                }  
            }
        }

        //Provjeri ima li zahtjeva (tag = 0) ili odgovora (tag = 1, iliti vilica koja se salje)
        static public bool imaliporuka(Intracommunicator comm, int susjed, int tag)
        {
            var status = comm.ImmediateProbe(susjed, tag);
            if (status != null) return true;
            return false;
        }
    }
}
