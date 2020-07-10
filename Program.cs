using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace DiagrameCFlux
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, List<string>> flx = extraireFlux("H:/Mes Fichiers/Ressources Programmation/lexique/dictEtDef.txt");
            Random r = new Random(1545);
            diagCirculaire(flx, 23000, 1000, 9000, 10000,"Arial",40).Save("H:/Mes Fichiers/Ressources Programmation/lexique/diag.bmp");
        }
        static Dictionary<string, List<string>> extraireFlux(string Path)
        {
            Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();
            string[] lignes = File.ReadAllLines(Path, Encoding.Default);
            foreach (string l in lignes)
            {
                if (l != "")
                {
                    KeyValuePair<string, List<string>> kpv = listeMotsDef(l);
                    if (!ret.ContainsKey(kpv.Key.ToLower()))
                    {
                        ret.Add(kpv.Key.ToLower(), kpv.Value);
                    }
                }
            }
            return ret;
        }
        static string TrimPunctuation(string value)
        {
            string retour = "";

            foreach (char c in value)
            {
                if (!char.IsPunctuation(c))
                {
                    retour += c;
                }
            }
            return retour;
        }
        public static KeyValuePair<string,List<string>> listeMotsDef(string def)
        {
            string[] mtetdef = def.Split(new char[] { '=' });
            string st = TrimPunctuation(mtetdef[1]);
            string mot = TrimPunctuation(mtetdef[0]);
            string[] mots = st.Split();
            List<string> ret = new List<string>();
            foreach (string s in mots)
            {
                string s_ = s.ToLower();
                if (!ret.Contains(s_) && s != "")
                {
                    ret.Add(s_);
                }
            }
            return new KeyValuePair<string, List<string>>(mot,ret);
        }
        public static double rad2Deg(double rad)
        {
            return 360.0 * rad / (2.0 * Math.PI);
        }

        static Color InverserCouleur(Color ColourToInvert)
        {
            int RGBMAX = 255;
            return Color.FromArgb(RGBMAX - ColourToInvert.R,
              RGBMAX - ColourToInvert.G, RGBMAX - ColourToInvert.B);
        }
        static Bitmap diagCirculaire(Dictionary<string,List<string>> flux,int Dim, int R0, int R1, int R2,string font, int maxFont)
        {
            Bitmap retour = new Bitmap(Dim, Dim);
            Random r = new Random();
            Tuple<List<ligne>, Dictionary<string, sectionCercle>> precalc = ligne.genererLignes(flux, ref r);
            List<ligne> lignes = precalc.Item1;
            Dictionary<string, sectionCercle> sections = precalc.Item2;
            using (Graphics gr = Graphics.FromImage(retour))
            {
                
               
                
                int nbAttaches = getNbAttaches(flux);
                int largeurConnexion = (int)((Math.PI * 2.0 * R1) / (double)nbAttaches);
                double shiftAngle = (largeurConnexion / 2.0) / (R1);
                List<int> indices = new List<int>();
                for(int i=0;i<lignes.Count;i++)
                {
                    indices.Add(i);
                }
                for(int n=0;n<lignes.Count;n++)
                {
                    int randsel = r.Next(indices.Count);
                    int indselect = indices.ElementAt(randsel);
                    indices.RemoveAt(randsel);
                    ligne l = lignes.ElementAt(indselect);
                    PointF p0 = new PointF((float)(Math.Cos(l.teta0+shiftAngle) * R1 + Dim / 2), (float)(Math.Sin(l.teta0 + shiftAngle) * R1+Dim/2));
                    PointF p0_ = new PointF((float)(Math.Cos(l.teta0 + shiftAngle) * (R1+R0)/2.0 + Dim / 2), (float)(Math.Sin(l.teta0 + shiftAngle) * (R1+R0)/2.0 + Dim / 2));
                    PointF p2_ = new PointF((float)(Math.Cos(l.teta1 + shiftAngle) * (R1+R0)/2.0 + Dim / 2), (float)(Math.Sin(l.teta1 + shiftAngle) * (R1+R0)/2.0 + Dim / 2));
                    PointF p2 = new PointF((float)(Math.Cos(l.teta1 + shiftAngle) * R1+Dim/2), (float)(Math.Sin(l.teta1 + shiftAngle) * R1+Dim/2));
                    Pen pen = new Pen(l.couleur, largeurConnexion);
                    PointF[] pts = new PointF[] { p0,p0_,p2_, p2 };
                    gr.DrawBeziers(pen, pts);
                    Console.WriteLine(n + " " + lignes.Count);
                }
                int epaisseurCercle = R2 - R1;
                int coteCarre = R2 * 2 - epaisseurCercle;
                
                double angleMin = 360.0 * 2.0 / (R1 * Math.PI * 2.0);
                Rectangle rect = new Rectangle((Dim - coteCarre) / 2, (Dim - coteCarre) / 2, coteCarre, coteCarre);
                int u = 0;
                Console.WriteLine("Sections");
                foreach (KeyValuePair<string, sectionCercle> kvp in sections)
                {
                    u++;
                    Pen pen = new Pen(kvp.Value.couleur, epaisseurCercle);
                    float swap = (float)Math.Max(rad2Deg(kvp.Value.tetaF - kvp.Value.tetaI),angleMin);
                    gr.DrawArc(pen, rect, (float)(rad2Deg(kvp.Value.tetaI)), swap);
                    ecrireSecteurAngulaire(gr, kvp.Value.tetaI, kvp.Value.tetaF, (R2+R1)/2, InverserCouleur(kvp.Value.couleur),kvp.Key, font, maxFont, Dim);
                    Console.WriteLine(u + " " + sections.Count);
                }
            }
            return retour;
        }
        static void ecrireSecteurAngulaire(Graphics gr, double teta0, double teta1,double Rayon,Color couleur,string texte,string fontName,int maxFont,int Dim)
        {
            bool inv = false;
            SolidBrush sb = new SolidBrush(couleur);
            if(Math.Cos(teta0)>Math.Cos(teta1))
            {
                inv = true;
            }
            
            int nbLettres = texte.Length;
            double fontSize = Rayon * (teta1 - teta0) / (double)nbLettres;
            fontSize = Math.Min(maxFont,Math.Max(1, fontSize));
            Font font = new Font(fontName, (int)(fontSize));
            
            for (int i=0;i<nbLettres;i++)
            {
                string lettre = "";
                if(!inv)
                {
                    lettre = texte[i].ToString();
                }
                else
                {
                    lettre = texte[nbLettres-i-1].ToString();
                }
                double teta = teta0 + ((i+1) / (double)(nbLettres+1)) * (teta1 - teta0);
                int x = 0;
                int y = 0;
                
                x = (int)((Rayon) * Math.Cos(teta)-fontSize/2);
                y = (int)((Rayon) * Math.Sin(teta)-fontSize/2);
                gr.DrawString(lettre, font, sb, new Point(x+Dim/2, y+Dim/2));
            }
        }
        static int getNbInput(Dictionary<string, List<string>> flux,string mot)
        {
            int nb = 0;
            for(int i=0;i< flux[mot].Count;i++)
            {
                if(flux.ContainsKey(flux[mot].ElementAt(i)))
                {
                    nb++;
                }
            }
            return nb;
        }
        static int getNbOutput(Dictionary<string, List<string>> flux, string mot)
        {
            int result = 0;
            foreach(KeyValuePair<string,List<string>> kvp in flux)
            {
                if(kvp.Value.Contains(mot))
                {
                    result++;
                }
            }
            return result;
        }
        static int getNbAttaches(Dictionary<string, List<string>> flux)
        {
            int result = 0;
            foreach (KeyValuePair<string, List<string>> kvp in flux)
            {
                List<string> defs = kvp.Value;
                for (int i = 0; i < defs.Count; i++)
                {
                    if (flux.ContainsKey(defs.ElementAt(i)))
                    {
                        result++;
                    }
                }
            }
            return 2*result;
        }
        static Color randomColor(ref Random r)
        {
            return Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
        }
        class sectionCercle
        {
            public double tetaI;
            public double tetaC;
            public double tetaF;
            public int nbInput;
            public int nbOutput;
            public int nbInputReel;
            public int nbOutputReel;
            public Color couleur;
            public sectionCercle(double teta0, int nbAttaches, Dictionary<string, List<string>> flux,string mot,ref Random r)
            {

                tetaI = teta0;
                nbInput = getNbInput(flux, mot);
                nbOutput = getNbOutput(flux, mot);
                tetaC = tetaI + Math.PI * 2.0 * (nbInput / (double)nbAttaches);
                tetaF = tetaC + Math.PI * 2.0 * (nbOutput / (double)nbAttaches);
                nbInputReel = 0;
                nbOutputReel = 0;
                couleur = randomColor(ref r);
            }
            public double getTetaInput()
            {
                return tetaI + ((nbInputReel) / (double)(nbInput)) * (tetaC - tetaI);
            }
            public double getTetaOutput()
            {
                return tetaC + ((nbOutputReel) / (double)(nbOutput)) * (tetaF - tetaC);
            }
            public static Dictionary<string,sectionCercle> genererSections(Dictionary<string, List<string>> flux, ref Random r)
            {
                Console.WriteLine("Generation des sections");
                Dictionary<string, sectionCercle> retour = new Dictionary<string, sectionCercle>();
                int nbAttaches = getNbAttaches(flux);
                double teta0 = 0;
                int n = 0;
                int sommeAttaches = 0;
                foreach(string mot in flux.Keys)
                {
                    n++;
                    sectionCercle sc = new sectionCercle(teta0, nbAttaches, flux, mot,ref r);
                    sommeAttaches += sc.nbInput + sc.nbOutput;
                    retour.Add(mot, sc);
                    teta0 = (sommeAttaches/(double)nbAttaches)*Math.PI*2;
                    Console.WriteLine(n+" "+flux.Keys.Count);
                }
                return retour;
            }

        }
        class ligne
        {
            public double teta0;
            public double teta1;
            public Color couleur;

            public ligne(double teta0, double teta1, Color couleur)
            {
                this.teta0 = teta0;
                this.teta1 = teta1;
                this.couleur = Color.FromArgb(128,couleur);
                
            }

            public static Tuple<List<ligne>, Dictionary<string, sectionCercle>> genererLignes(Dictionary<string, List<string>> flux, ref Random r)
            {
                List<ligne> retour = new List<ligne>();
                
                Dictionary<string, sectionCercle> sections = sectionCercle.genererSections(flux, ref r);
                Console.WriteLine("Enregistrement des lignes");
                int n = 0;
                foreach (KeyValuePair<string, List<string>> kvp in flux)
                {
                    string motCible = kvp.Key;
                    foreach(string motSource in kvp.Value)
                    {
                        if (sections.ContainsKey(motSource))
                        {
                            ligne nl = new ligne(sections[motSource].getTetaOutput(), sections[motCible].getTetaInput(), sections[motSource].couleur);
                            retour.Add(nl);
                            sections[motCible].nbInputReel++;
                            sections[motSource].nbOutputReel++;
                        }
                    }
                    n++;
                    Console.WriteLine(n + " " + flux.Keys.Count);
                }
                return new Tuple<List<ligne>, Dictionary<string, sectionCercle>>(retour,sections);
            }
        }
    }
}
