﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace SimuladorHorario
{
    
    public enum TipoEvento { PRBA, CLAS, LABT, AYUD, EXTRAP, PERS }
    public enum Especialidad { ING, IOC, ICE, ICC, ICI, ICA }
    public enum TipoCurso { Curricular, Extra }
    public enum Concentracion { AplicacionesWeb, Algoritmos, Modelacion, Bioprocesos, Hidraulica, Señales }
    public enum FormatoImpresion { Negativo, Positivo, Normal }
    public enum BloquesHorarios { i8_30, i9_30, i10_30, i11_30, i12_30, i13_30, i14_30, i15_30, i16_30, i17_30, i18_30, i19_30, i20_30 }

    [Serializable()]
    public static class Aplicacion
    {
        public static List<Usuario> usuarios = new List<Usuario>();
        public static List<CursoCurricular> cursos = new List<CursoCurricular>();
        public static List<string> cursosPreRequisito = new List<string>();
        public static Usuario usuarioActual;


        public static List<CursoCurricular> GetCursoCurricular() { return cursos; }
        

        public static Usuario IniciarSesion(string nombreUsuario, string contraseña)
        {
           
            foreach (Usuario usuario in usuarios)
            {
                
                if (usuario is Estudiante)
                {
                    Estudiante estudiante = (Estudiante)usuario;
                    if (usuario.nombre == nombreUsuario && usuario.contraseña == contraseña)
                    {
                        usuarioActual = usuario;
                        return usuarioActual;
                    }
                }
                else if (usuario is Administrador)
                {
                    Administrador administrador = (Administrador)usuario;
                    if (usuario.nombre == nombreUsuario && usuario.contraseña == contraseña)
                    {
                        usuarioActual = usuario;
                        return usuarioActual;
                    }
                }
            }

            return null;
        }

        public static bool RegistrarUsuario(string nombreUsuario, string contraseña, Concentracion concentracion ,Especialidad especialidad)
        {

            if (NombresUsuarios().Contains(nombreUsuario))
            {
                return false;
            }

            List <string>avanceMalla = new List<string>();
            #region
            /*
            Console.WriteLine("Ingrese los cursos de su avance de malla");
            List<CursoCurricular> cursosCurriculares = GetCursosCurriculares();
            
            bool flag = true;
            int cont = 0;
            do
            {
                for (int i = 1; i < cursosCurriculares.Count(); i++)
                {

                    if (cursos[i].tipo.Equals(TipoCurso.Curricular))
                    {
                        Console.WriteLine(i + ". " + cursosCurriculares[i-1].nombre);
                        cont++;
                    }
                }
                Console.WriteLine("Ingrese su curso:");
                int opcion2 = Program.ChequearOpcion(1, cont);
                avanceMalla.Add(cursosCurriculares[opcion2-1]);
                Program.ImprimirPositivo("Curso agregado a su avance de malla");
                Console.WriteLine("¿Desea agregar otro curso?\n1. Si\n2. No");
                opcion2 = Program.ChequearOpcion(1, 2);
                if (opcion2 == 2) flag = false;
            } while (flag); */
            #endregion
            
            usuarios.Add(new Estudiante(avanceMalla, especialidad, concentracion, nombreUsuario, contraseña, false));
            return true;
        }
        

        public static List<string> NombresUsuarios()
        {
            List<string> retorno = new List<string>();
            foreach (Usuario usuario in usuarios)
            {
                if (usuario != null)
                {
                    retorno.Add(usuario.nombre);
                }
            }
            return retorno;
        }


        public static void CargarCursos(string fileName = "dataCursosDisponibles.csv")
        {
            try
            {
                //Encuentra el directorio donde se encuentra el archivo csv de cursos
                string path = Path.GetFullPath(@"..\..");
                path = Path.Combine(path, "archivos");
                Directory.CreateDirectory(path);
                Program.ImprimirPositivo("Cursos:\tDir: " + path);
                path = Path.Combine(path, fileName);

                StreamReader file = new StreamReader(path);
                string line;
                string previoNRC = string.Empty;
                string nombre, profesor, nrc, carrera;
                int creditos;
                List<string> cursosPreRequisito = new List<string>();


                //El csv posee varias lineas de un mismo curso. Esto genera conjuntos de la lineas
                //del mismo curso.
                List<List<string>> conjuntoCursosDistintos = new List<List<string>>();
                conjuntoCursosDistintos.Add(new List<string>() { file.ReadLine() });
                while ((line = file.ReadLine()) != null)
                {
                    string[] datosLine = line.Split(';');
                    nrc = datosLine[0];
                    if (previoNRC == nrc || previoNRC == "")
                    {
                        conjuntoCursosDistintos[conjuntoCursosDistintos.Count()-1].Add(line);
                    }
                    else
                    {
                        conjuntoCursosDistintos.Add(new List<string>());
                        conjuntoCursosDistintos[conjuntoCursosDistintos.Count() - 1].Add(line);
                    }
                    previoNRC = nrc;
                }


                foreach(List<string> curso in conjuntoCursosDistintos)
                {
                    List<Evento> listaEvento = new List<Evento>();
                    List<string> listaHorariosBloques = new List<string>();
                    int contadorLineaCurso = 1;
                    foreach (string linea in curso)
                    {
                        string[] datosLinea = linea.Split(';');
                        List<string> listaHorarioLinea = new List<string>();    //listaHorarioLinea es el conjunto de horarios que se encuentra en una linea 
                        for(int i = 6; i < 12; i++)
                        {
                            if(datosLinea[i] != "") //Si en la casilla existe un horario entonces...
                            {
                                string fecha = datosLinea[12].Replace('-',':').Replace('/',':');
                                if(datosLinea[12] == "") { fecha = "A"; }
                                listaHorarioLinea.Add((datosLinea[14] + ":" + (i - 6) + ":" + datosLinea[i] + ":" + fecha).Replace(" -", ":"));
                            }
                        }
                        string sala = datosLinea[13];
                        List<string> bloquesHorario = new List<string>();

                        for (int i = 0; i < listaHorarioLinea.Count; i++)
                        {
                            string stringHorario = string.Empty;
                            stringHorario = listaHorarioLinea[i];
                            listaEvento.AddRange(generarEvento(stringHorario,sala));
                        }   
                        
                        if (contadorLineaCurso == curso.Count)
                        {
                            //cursosPreRequisito.Clear();
                            string[] datosLinea2 = linea.Split(';');
                            cursosPreRequisito = (datosLinea2[16]).Split(',').ToList();
                            cursosPreRequisito.Remove("");
                            /*
                            foreach (string cur in datosCursosPreReq)
                            {
                                cursosPreRequisito.Add(cur);  
                            }*/
                            /*
                            for (int c = 0; c < cursosPreRequisito.Count; c++)
                            {
                                if (cursosPreRequisito[c] != "")
                                    System.Windows.Forms.MessageBox.Show(cursosPreRequisito[c]);
                            }
                            System.Windows.Forms.MessageBox.Show("FINNNNNN");
                            */
                            nombre = datosLinea2[4];
                            profesor = datosLinea2[15];
                            nrc = datosLinea2[0];
                            carrera = datosLinea2[1];
                            creditos = Convert.ToInt32(datosLinea2[5]);
                            //System.Windows.Forms.MessageBox.Show((cursosPreRequisito.Count).ToString());
                            CursoCurricular cursoCurricular = new CursoCurricular(nrc, creditos, cursosPreRequisito,
                                (Especialidad)Enum.Parse(typeof(Especialidad), carrera), listaEvento, nombre, profesor, TipoCurso.Curricular);
                            cursos.Add(cursoCurricular);
                        }
                        contadorLineaCurso++;
                    }

                }

                List<Evento> generarEvento(string stringHorario, string sala)
                {

                    //Console.WriteLine(stringHorario);Console.ReadKey();
                    //ingresa string del tipo           PRBA:D:8:30:11:20:A  ||  PRBA:D:8:30:11:20:20:03:2018
                    //retorna List<Evento>   
                    string nombreEvento = string.Empty;
                    int diaSemana = Convert.ToInt32(stringHorario.Split(':')[1]);
                    int horaInicio = Convert.ToInt32(stringHorario.Split(':')[2]);
                    int horaTermino = Convert.ToInt32(stringHorario.Split(':')[4]);

                    int cantBloques = horaTermino - horaInicio;
                    List<Evento> returnListaEventos = new List<Evento>();

                    string tipoEvento = (stringHorario.Split(':')[0]);
                    TipoEvento tipo_Evento = (TipoEvento)System.Enum.Parse(typeof(TipoEvento), tipoEvento);

                    if (stringHorario.Split(':')[6] != "A") //Si posee una fecha entonces....
                    {
                        string fechaDia = (stringHorario.Split(':')[6]);
                        string fechaMes = (stringHorario.Split(':')[7]);
                        string fechaAño;
                        try
                        {
                            fechaAño = (stringHorario.Split(':')[8]);
                        }
                        catch
                        {
                            fechaAño = "2018";
                        }
                        string fecha = fechaDia + "-" + fechaMes + "-" + fechaAño;

                        for (int i = 0; i < cantBloques; i++)
                        {
                            string inicioBloque = $"{diaSemana}-{horaInicio + i}:30";
                            Evento evento = new Evento(nombreEvento, inicioBloque, fecha, sala, tipo_Evento);

                            if (i == 0)
                            {
                                evento.primerPeriodo = true;
                            }

                            returnListaEventos.Add(evento);
                        }
                        return returnListaEventos;
                    }
                    else
                    {
                        for (int i = 0; i < cantBloques; i++)
                        {
                            string inicioBloque = $"{diaSemana}-{horaInicio + i}:30";
                            Evento evento = new Evento(nombreEvento, inicioBloque, sala, tipo_Evento);
                            if (i == 0)
                            {
                                evento.primerPeriodo = true;
                            }
                            returnListaEventos.Add(evento);
                        }
                        return returnListaEventos;
                    }
                }

                file.Close();
            }
            catch (FileNotFoundException e)
            {
                System.Windows.Forms.MessageBox.Show("Error de lectura cargar Cursos","ERROR");
            }
        }

        public static void CargarUsuarios(string fileName = "saveData.csv")
        {
            try
            {
                string path = Path.GetFullPath(@"..\..");
                path = Path.Combine(path, "archivos");
                Directory.CreateDirectory(path);
                Program.ImprimirPositivo("Users:\tDir: " + path);
                path = Path.Combine(path, fileName);

                StreamReader file = new StreamReader(path);
                string linea;
                while ((linea = file.ReadLine()) != null)
                {
                    string[] lineaSeparada = linea.Split(';');
                    string nombre = lineaSeparada[0];
                    string contraseña = lineaSeparada[1];
                    bool admin;
                    
                    if (lineaSeparada[2] == "true")
                    {
                        admin = true;
                        Administrador administrador = new Administrador(nombre, contraseña, admin);
                        usuarios.Add(administrador);
                    }
                    else
                    {
                        string especialidad, añoIngreso, concentracion, avanceMalla;
                        List<string> listaAvanceMalla;
                        especialidad = lineaSeparada[4];
                        añoIngreso = lineaSeparada[5];
                        concentracion = lineaSeparada[6];
                        avanceMalla = lineaSeparada[7];
                        listaAvanceMalla = avanceMalla.Split(',').ToList();
                        /*
                        for (int q = 0; q < listaAvanceMalla.Count; q++)
                            System.Windows.Forms.MessageBox.Show(listaAvanceMalla[q]);
                        */
                        /*
                        List<CursoCurricular> listaAvanceMalla = new List<string>();
                        foreach (string nrc in lineaSeparada[7].Split(','))
                        {
                            CursoCurricular curso = cursos.Find(x => x.nrc == nrc);
                            listaAvanceMalla.Add(curso);
                        }
                        */
                        Estudiante estudiante = new Estudiante(listaAvanceMalla, Especialidad.ICA, Concentracion.Algoritmos, nombre, contraseña, false);
                        usuarios.Add(estudiante);
                    }
                }
                file.Close();
            }
            catch (FileNotFoundException e1)
            {
                System.Windows.Forms.MessageBox.Show("Error de lectura cargar Usuarios","ERROR");
            }
        }

        public static void MostrarUsuarios()
        {
            Console.Clear();
            Program.ImprimirBanner("Imprimir Usuarios\n");
            foreach(Usuario usuario in usuarios)
            {
                Console.WriteLine($"Nombre: {usuario.nombre}, Admin: {usuario.esAdmin}");
            }
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        public static void GuardarData(string fileName = "saveData.csv")
        {

            string path = Path.GetFullPath(@"..\..");
            path = Path.Combine(path, "archivos");
            Directory.CreateDirectory(path);
            Program.ImprimirPositivo("Dir: " + path);
            path = Path.Combine(path,fileName);

            StreamWriter saveFile = new StreamWriter(path);
            foreach(Usuario usuario in usuarios)
            {
                saveFile.Write($"{usuario.nombre};{usuario.contraseña};");

                if (usuario is Estudiante)
                {
                    Estudiante estudiante = (Estudiante)usuario;
                    saveFile.Write($"false;ING;{estudiante.especialidad};0000;{estudiante.concentracion};1,2,3,4,5,6\n");
                }
                else
                {
                    saveFile.Write("true\n");
                }
            }
            saveFile.Close();
            Program.ImprimirPositivo("Guardado Exitoso.\n");
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
            return;
        }

        public static void AñadirCurso(CursoCurricular curso)
        {
            cursos.Add(curso);
        }

        public static void SerializeAll()
        {
            try
            {
                using (Stream stream = File.Open("../../archivos/usuariosData.bin", FileMode.OpenOrCreate))
                {
                    BinaryFormatter usuariosData = new BinaryFormatter();
                    usuariosData.Serialize(stream, usuarios);
                }
            }
            catch (IOException) { MessageBox.Show("No se pudo serializar", "Error"); }
                       
            try
            {
                using (Stream stream = File.Open("../../archivos/cursosData.bin", FileMode.OpenOrCreate))
                {
                    BinaryFormatter cursosData = new BinaryFormatter();
                    cursosData.Serialize(stream, cursos);
                }
            }
            catch (IOException) { MessageBox.Show("No se pudo serializar", "Error"); }

        }

        public static void DeSerializeAll()
        {
            try
            {
                using (Stream stream = File.Open("../../archivos/usuariosData.bin", FileMode.Open))
                {
                    BinaryFormatter usuariosData = new BinaryFormatter();

                    var usuariosSerializated = (List<Usuario>)usuariosData.Deserialize(stream);
                    foreach(Usuario user in usuariosSerializated)
                    {
                        if(user is Estudiante)
                        {
                            Estudiante estudiante = (Estudiante)user;
                            
                        }
                        else if(user is Administrador)
                        {
                            Administrador admin = (Administrador)user;
                        }
                        usuarios.Add(user);
                        
                    }
                }
            }
            catch (IOException) { }

            try
            {
                using (Stream stream = File.Open("../../archivos/cursosData.bin", FileMode.Open))
                {
                    BinaryFormatter cursosData = new BinaryFormatter();

                    var cursosSerializated = (List<CursoCurricular>)cursosData.Deserialize(stream);
                    foreach (CursoCurricular curs in cursosSerializated)
                    {
                        cursos.Add(curs);
                    }
                }
            }
            catch (IOException) { MessageBox.Show("No se pudo deserializar los cursos", "Error"); }
        }


    }
}
