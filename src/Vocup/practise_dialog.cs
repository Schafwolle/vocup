using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Vocup
{
    public partial class practise_dialog : Form
    {

        public int anz_vok;
        public int anz_geübt;
        public int anz_noch_nicht;
        public int anz_richtig = 0;
        public int anz_falsch = 0;
        public int anz_teilweise = 0;

        public string[,] practise_list;

        bool check;

        bool solution;

        public string uebersetzungsrichtung;

        string reset_fokus = "";

        public specialchars sonderzeichen_dialog = new specialchars();

        SimpleAudioVideoPlayback.MCIPlayback mci = new SimpleAudioVideoPlayback.MCIPlayback();


        public practise_dialog()
        {
            InitializeComponent();

        }

        //Form laden
        private void practise_dialog_Load(object sender, EventArgs e)
        {

            //Form vorbereiten

            anzahl_üben.Text = Convert.ToString(anz_vok);
            anzahl_geübt.Text = "0";
            anzahl_noch_nicht.Text = anzahl_üben.Text;
            anzahl_richtig.Text = "0";
            anzahl_teilweise.Text = "0";
            anzahl_falsch.Text = "0";

            progressBar.Maximum = anz_vok;

            if (Properties.Settings.Default.selber_bewerten)
            {
                selber_bewerten_groupbox.Visible = true;
                selber_bewerten_groupbox.Enabled = false;
            }

            //Erste Vokabel einlesen

            set_vokabel();
        }

        //Falls auf den Fortfahren-Button geklickt wurde
        private void fortfahren_button_Click(object sender, EventArgs e)
        {
            if (anz_geübt == anz_vok)
            {
                mci.Close();
                Close();
            }

            else if (Properties.Settings.Default.selber_bewerten == true)
            {
                if (check == false)
                {
                    set_vokabel();
                }
                else if (check == true && solution == true)
                {
                    show_solution();
                }
                else if (check == true && solution == false)
                {
                    check_vokabel();

                    if (anz_vok != anz_geübt)
                    {
                        set_vokabel();
                    }
                    else
                    {
                        Close();
                    }
                }

            }

            else if (Properties.Settings.Default.only_one_click == true && Properties.Settings.Default.selber_bewerten == false)
            {
                check_vokabel();
                if (anz_vok != anz_geübt)
                {
                    set_vokabel();
                }
            }

            //Falls die Eingabe überprüft werden soll

            else if (check == true)
            {
                check_vokabel();
            }

            //Falls die nächste vokabel eingelesen werden soll

            else
            {
                set_vokabel();
            }

        }

        //Eine Vokabel lernen

        public void set_vokabel()
        {
            //Vokabel einlesen

            vokabel_foreign_language.Text = "";
            vokabel_own_language.Text = "";
            vokabel_synonym.Text = "";

            if (Properties.Settings.Default.selber_bewerten == true)
            {
                selber_bewerten_groupbox.Enabled = false;
            }

            if (Properties.Settings.Default.only_one_click != true || Properties.Settings.Default.selber_bewerten == true)
            {
                correction_box.Text = "";
                correction_box.BackColor = SystemColors.Control;
            }
            //Falls von Fremdsprache nach Muttersprache geübt werden soll
            if (uebersetzungsrichtung == "2")
            {
                vokabel_foreign_language.Text = practise_list[anz_geübt, 3];

                vokabel_own_language.Select();

                vokabel_synonym.Text = practise_list[anz_geübt, 4];

                vokabel_own_language.ReadOnly = false;
                vokabel_foreign_language.ReadOnly = true;
                vokabel_synonym.ReadOnly = true;

                //Eingabe-Felder mit Farbe hervorheben

                if (Properties.Settings.Default.colored_textfields == true)
                {
                    vokabel_own_language.BackColor = Color.FromArgb(250, 250, 150);
                }

                vokabel_foreign_language.BackColor = DefaultBackColor;
                vokabel_synonym.BackColor = DefaultBackColor;
            }

            //Falls von Muttersprache nach Fremdsprache geübt werden soll
            else
            {
                vokabel_own_language.Text = practise_list[anz_geübt, 2];

                if (practise_list[anz_geübt, 4] == "" || practise_list[anz_geübt, 4] == null)
                {
                    vokabel_synonym.ReadOnly = true;

                    vokabel_synonym.BackColor = DefaultBackColor;
                }
                else
                {
                    vokabel_synonym.ReadOnly = false;

                    if (Properties.Settings.Default.colored_textfields == true)
                    {
                        vokabel_synonym.BackColor = Color.FromArgb(250, 250, 150);
                    }
                }

                vokabel_own_language.ReadOnly = true;
                vokabel_own_language.BackColor = DefaultBackColor;
                vokabel_foreign_language.ReadOnly = false;
                if (Properties.Settings.Default.colored_textfields == true)
                {
                    vokabel_foreign_language.BackColor = Color.FromArgb(250, 250, 150);
                }
                vokabel_foreign_language.Select();
            }

            check = true;
            solution = true;
        }

        //Eingabe kontrollieren
        private void check_vokabel()
        {

            vokabel_foreign_language.ReadOnly = true;
            vokabel_own_language.ReadOnly = true;
            vokabel_synonym.ReadOnly = true;

            selber_bewerten_groupbox.Enabled = true;

            //Eingabe auf Richtigkeit überprüfen

            bool[] correct = new bool[2];

            if (Properties.Settings.Default.selber_bewerten == false)
            {
                if (uebersetzungsrichtung == "2")
                {

                    string vokabel_own_language_komp = prepare_text(vokabel_own_language.Text);
                    string vokabel_own_language_komp_richtig = prepare_text(practise_list[anz_geübt, 2]);


                    //Richtig
                    if (vokabel_own_language.Text == practise_list[anz_geübt, 2] || vokabel_own_language.Text == practise_list[anz_geübt, 2] + " ")
                    {
                        correct[0] = true;
                        correct[1] = true;
                    }


                    //Teilweise richtig
                    else if (vokabel_own_language_komp == vokabel_own_language_komp_richtig)
                    {
                        correct[0] = true;
                        correct[1] = false;

                    }
                    //Richtig wenn zwei mit , oder ; getrennte Wörter vertauscht worden sind

                    else if (practise_list[anz_geübt, 2].Contains(",") || practise_list[anz_geübt, 2].Contains(";"))
                    {
                        string[] keywords = practise_list[anz_geübt, 2].Replace(" ", "").Replace(";", ",").Split(',');

                        for (int i = 0; i < keywords.Length; i++)
                        {
                            if (vokabel_own_language.Text.Contains(keywords[i]) == true)
                            {
                                correct[0] = true;
                                correct[1] = true;
                            }
                            else
                            {
                                correct[0] = false;
                                correct[1] = false;
                                break;
                            }
                        }
                    }
                    //Falsch
                    else
                    {
                        correct[0] = false;
                        correct[1] = false;
                    }
                }
                else
                {
                    //Falls kein Synonym vorhanden ist
                    if (practise_list[anz_geübt, 4] == "" || practise_list[anz_geübt, 4] == null)
                    {

                        string vokabel_foreign_language_komp_richtig = prepare_text(practise_list[anz_geübt, 3]);
                        string vokabel_foreign_language_komp = prepare_text(vokabel_foreign_language.Text);


                        //Richtig
                        if (vokabel_foreign_language.Text == practise_list[anz_geübt, 3] || vokabel_foreign_language.Text == practise_list[anz_geübt, 3] + " ")
                        {
                            correct[0] = true;
                            correct[1] = true;
                        }

                        //Teilweise richtig
                        else if (vokabel_foreign_language_komp == vokabel_foreign_language_komp_richtig)
                        {
                            correct[0] = true;
                            correct[1] = false;
                        }
                        //Richtig wenn zwei mit , oder ; getrennte Wörter vertauscht worden sind

                        else if (practise_list[anz_geübt, 3].Contains(",") || practise_list[anz_geübt, 3].Contains(";"))
                        {
                            string[] keywords = practise_list[anz_geübt, 3].Replace(" ", "").Replace(";", ",").Split(',');


                            for (int i = 0; i < keywords.Length; i++)
                            {
                                if (vokabel_foreign_language.Text.Contains(keywords[i]) == true)
                                {
                                    correct[0] = true;
                                    correct[1] = true;
                                }
                                else
                                {
                                    correct[0] = false;
                                    correct[1] = false;
                                    break;
                                }
                            }
                        }
                        //Falsch
                        else
                        {
                            correct[0] = false;
                            correct[1] = false;
                        }

                    }
                    //Falls ein Synonym vorhanden ist
                    else
                    {
                        string vokabel_foreign_language_komp_richtig = prepare_text(practise_list[anz_geübt, 3]);
                        string vokabel_foreign_language_komp = prepare_text(vokabel_foreign_language.Text);
                        string vokabel_synonym_komp = prepare_text(vokabel_foreign_language.Text);
                        string vokabel_synonym_komp_richtig = prepare_text(practise_list[anz_geübt, 4]);

                        //Richtig
                        if (vokabel_foreign_language.Text == practise_list[anz_geübt, 3] && vokabel_synonym.Text == practise_list[anz_geübt, 4])
                        {
                            correct[0] = true;
                            correct[1] = true;
                        }
                        else if (vokabel_foreign_language.Text == practise_list[anz_geübt, 4] && vokabel_synonym.Text == practise_list[anz_geübt, 3])
                        {
                            correct[0] = true;
                            correct[1] = true;
                        }

                        //Teilweise richtig

                        else if (Properties.Settings.Default.nearly_correct_synonym == true)
                        {

                            if (vokabel_foreign_language.Text == practise_list[anz_geübt, 3] && vokabel_synonym.Text != practise_list[anz_geübt, 4])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                            else if (vokabel_foreign_language.Text == practise_list[anz_geübt, 4] && vokabel_synonym.Text != practise_list[anz_geübt, 3])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                            else if (vokabel_foreign_language.Text != practise_list[anz_geübt, 3] && vokabel_synonym.Text == practise_list[anz_geübt, 4])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                            else if (vokabel_foreign_language.Text != practise_list[anz_geübt, 4] && vokabel_synonym.Text == practise_list[anz_geübt, 3])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                        }

                        else if (vokabel_foreign_language_komp == vokabel_foreign_language_komp_richtig && vokabel_synonym_komp == vokabel_synonym_komp_richtig)
                        {
                            correct[0] = true;
                            correct[1] = false;
                        }
                        else if (vokabel_foreign_language_komp == vokabel_synonym_komp_richtig && vokabel_synonym_komp == vokabel_foreign_language_komp_richtig)
                        {
                            correct[0] = true;
                            correct[1] = false;
                        }

                        //Richtig wenn zwei mit , oder ; getrennte Wörter vertauscht worden sind

                        else if (practise_list[anz_geübt, 3].Contains(",") == true || practise_list[anz_geübt, 3].Contains(";") == true)
                        {
                            string[] keywords = practise_list[anz_geübt, 3].Replace(" ", "").Replace(";", ",").Split(',');


                            for (int i = 0; i < keywords.Length; i++)
                            {
                                if (vokabel_foreign_language.Text.Contains(keywords[i]) == true)
                                {
                                    correct[0] = true;
                                    correct[1] = true;
                                }
                                else
                                {
                                    correct[0] = false;
                                    correct[1] = false;
                                    break;
                                }
                            }
                        }

                        //falsch
                        else
                        {
                            correct[0] = false;
                            correct[1] = false;
                        }
                    }
                }
            }

            //Falls selber bewertet werden soll
            else
            {
                if (radio_korrekt.Checked == true)
                {
                    correct[0] = true;
                    correct[1] = true;
                }
                else if (radio_teilweise_korrekt.Checked == true)
                {
                    correct[0] = true;
                    correct[1] = false;
                }
                else
                {
                    correct[0] = false;
                    correct[1] = false;
                }
            }

            //Ergebnis bekannt geben

            if (correct[0] == true && correct[1] == true)
            {
                if (Properties.Settings.Default.selber_bewerten == false)
                {

                    correction_box.Text = Properties.language.practise_right;
                    correction_box.BackColor = Color.FromArgb(144, 238, 144);
                }
                anz_richtig++;

                //Ergebnisse speichern

                if (Convert.ToInt32(practise_list[anz_geübt, 1]) == 0)
                {
                    practise_list[anz_geübt, 1] = Convert.ToString(2);
                }
                else
                {
                    practise_list[anz_geübt, 1] = Convert.ToString(Convert.ToInt32(practise_list[anz_geübt, 1]) + 1);
                }

                practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                practise_list[anz_geübt, 6] = "right";

                if (Properties.Settings.Default.selber_bewerten == false)
                {

                    //Sound abspielen

                    mci.Close();

                    if (Properties.Settings.Default.sound == true)
                    {
                        FileInfo info = new FileInfo(Application.StartupPath + @"\" + "sound_correct.wav");

                        if (info.Exists == true)
                        {

                            mci.Open(Application.StartupPath + @"\" + "sound_correct.wav", "mpegvideo");
                            mci.SetTimeFormat("ms");

                            mci.Play();
                        }
                    }
                }
            }

            else if (correct[0] == true && correct[1] == false)
            {
                if (uebersetzungsrichtung == "2")
                {
                    if (Properties.Settings.Default.selber_bewerten == false)
                    {

                        correction_box.Text = Properties.language.practise_nearly_right + practise_list[anz_geübt, 2] + ")";
                    }
                    //Ergebnisse speichern

                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "nearly right";
                    practise_list[anz_geübt, 7] = vokabel_own_language.Text;
                }

                else
                {
                    if (Properties.Settings.Default.selber_bewerten == false)
                    {

                        correction_box.Text = Properties.language.practise_nearly_right + practise_list[anz_geübt, 3];
                        if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null)
                        {
                            correction_box.Text = correction_box.Text + " " + Properties.language.and + " " + practise_list[anz_geübt, 4];
                        }
                        correction_box.Text = correction_box.Text + ")";
                    }
                    //Ergebnisse speichern

                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "nearly right";
                    practise_list[anz_geübt, 7] = vokabel_foreign_language.Text;
                    if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null && vokabel_synonym.Text != "")
                    {
                        practise_list[anz_geübt, 7] = practise_list[anz_geübt, 7] + ", " + vokabel_synonym.Text;
                    }

                }

                anz_teilweise++;

                if (Properties.Settings.Default.selber_bewerten == false)
                {
                    correction_box.BackColor = Color.FromArgb(255, 215, 0);


                    //Sound abspielen

                    mci.Close();

                    if (Properties.Settings.Default.sound == true)
                    {
                        FileInfo info_correct = new FileInfo(Application.StartupPath + @"\" + "sound_correct.wav");
                        FileInfo info_nearly_correct = new FileInfo(Application.StartupPath + @"\" + "sound_nearly_correct.wav");

                        if (info_nearly_correct.Exists == true)
                        {
                            mci.Open(Application.StartupPath + @"\" + "sound_nearly_correct.wav", "mpegvideo");
                            mci.SetTimeFormat("ms");

                            mci.Play();
                        }
                        else if (info_correct.Exists == true)
                        {
                            mci.Open(Application.StartupPath + @"\" + "sound_correct.wav", "mpegvideo");
                            mci.SetTimeFormat("ms");

                            mci.Play();
                        }
                    }
                }
            }

            else
            {
                if (uebersetzungsrichtung == "2")
                {
                    if (Properties.Settings.Default.selber_bewerten == false)
                    {
                        correction_box.Text = Properties.language.practise_false + practise_list[anz_geübt, 2] + ")";
                    }
                    //Ergebnisse speichern

                    practise_list[anz_geübt, 1] = "1";
                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "false";
                    practise_list[anz_geübt, 7] = vokabel_own_language.Text;
                }

                else
                {
                    if (Properties.Settings.Default.selber_bewerten == false)
                    {

                        correction_box.Text = Properties.language.practise_false + practise_list[anz_geübt, 3];
                        if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null)
                        {
                            correction_box.Text = correction_box.Text + " " + Properties.language.and + " " + practise_list[anz_geübt, 4];
                        }
                        correction_box.Text = correction_box.Text + ")";
                    }
                    //Ergebnisse speichern

                    practise_list[anz_geübt, 1] = "1";
                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "false";
                    practise_list[anz_geübt, 7] = vokabel_foreign_language.Text;
                    if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null & vokabel_synonym.Text != "")
                    {
                        practise_list[anz_geübt, 7] = practise_list[anz_geübt, 7] + ", " + vokabel_synonym.Text;
                    }
                }
                anz_falsch++;

                if (Properties.Settings.Default.selber_bewerten == false)
                {

                    correction_box.BackColor = Color.FromArgb(255, 192, 203);


                    //Sound abspielen

                    mci.Close();

                    if (Properties.Settings.Default.sound == true)
                    {
                        FileInfo info = new FileInfo(Application.StartupPath + @"\" + "sound_wrong.wav");

                        if (info.Exists == true)
                        {
                            mci.Open(Application.StartupPath + @"\" + "sound_wrong.wav", "mpegvideo");
                            mci.SetTimeFormat("ms");

                            mci.Play();
                        }
                    }
                }
            }

            //Nächste Vokabel vorbereiten
            if (anz_geübt != anz_vok)
            {
                anz_geübt++;
            }

            //Zahlen aktualisieren

            refresh_numbers();

            check = false;
            solution = false;

            if (anz_vok == anz_geübt)
            {
                fortfahren_button.Text = Properties.language.finish;
            }
        }

        //Lösung anzeigen falls die Lösung selber überprüft werden soll

        private void show_solution()
        {
            //Lösung anzeigen

            if (uebersetzungsrichtung == "2")
            {
                correction_box.Text = practise_list[anz_geübt, 2];
            }
            else
            {
                correction_box.Text = practise_list[anz_geübt, 3];
                if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null)
                {
                    correction_box.Text = correction_box.Text + " " + Properties.language.and + " " + practise_list[anz_geübt, 4];
                }

            }

            correction_box.BackColor = Color.White;

            vokabel_foreign_language.ReadOnly = true;
            vokabel_own_language.ReadOnly = true;
            vokabel_synonym.ReadOnly = true;

            //Radio-Buttons aktivieren

            selber_bewerten_groupbox.Enabled = true;


            check = true;
            solution = false;
        }

        //Zahlen aktualisieren

        private void refresh_numbers()
        {
            //Progress-Bar

            progressBar.Value = anz_geübt;

            //Zahlen aktualiseren

            anzahl_geübt.Text = Convert.ToString(anz_geübt);
            anzahl_noch_nicht.Text = Convert.ToString(anz_vok - anz_geübt);
            anzahl_richtig.Text = Convert.ToString(anz_richtig);
            anzahl_teilweise.Text = Convert.ToString(anz_teilweise);
            anzahl_falsch.Text = Convert.ToString(anz_falsch);

        }

        //Sonderzeichen

        private void vokabel_own_language_Enter(object sender, EventArgs e)
        {
            reset_fokus = "own_language";
        }

        private void vokabel_foreign_language_Enter(object sender, EventArgs e)
        {
            reset_fokus = "foreign_language";
        }

        private void vokabel_synonym_Enter(object sender, EventArgs e)
        {
            reset_fokus = "synonym";
        }

        private void sonderzeichen_button_Click(object sender, EventArgs e)
        {
            if (sonderzeichen_dialog.OwnedForms.Length == 0)
            {

                //Events definieren

                sonderzeichen_dialog.choose_char += add_special_char;

                sonderzeichen_dialog.FormClosed += sonderzeichen_dialog_closed;



                //Position des Fensters festlegen

                sonderzeichen_dialog.Left = this.Left + (this.Width - sonderzeichen_dialog.Width) / 2;
                sonderzeichen_dialog.Top = this.Top + this.Height + 10;


                //mForm als Besitzer festlegen

                sonderzeichen_dialog.Owner = this.Owner;

                sonderzeichen_dialog.Show();


                //Sonderzeichen-Button deaktivieren

                sonderzeichen_button.Enabled = false;
            }
        }

        //Sonderzeichen-Dialog

        private void sonderzeichen_dialog_closed(object sender, EventArgs e)
        {
            sonderzeichen_button.Enabled = true;
            sonderzeichen_dialog = new specialchars();
        }

        private void add_special_char(object sender, EventArgs e)
        {
            Button aktuellerButton = (Button)sender;

            // Button-Text in die Zwischenablage und in das Textfeld kopieren kopieren

            Clipboard.SetText(aktuellerButton.Text);

            switch (reset_fokus)
            {
                case "own_language":

                    vokabel_own_language.Focus();

                    if (vokabel_own_language.ReadOnly == false)
                    {
                        vokabel_own_language.Text = vokabel_own_language.Text + Clipboard.GetText();
                    }
                    vokabel_own_language.SelectionStart = vokabel_own_language.TextLength;


                    break;

                case "foreign_language":


                    vokabel_foreign_language.Focus();

                    if (vokabel_foreign_language.ReadOnly == false)
                    {
                        vokabel_foreign_language.Text = vokabel_foreign_language.Text + Clipboard.GetText();
                    }
                    vokabel_foreign_language.SelectionStart = vokabel_foreign_language.TextLength;


                    break;

                case "synonym":

                    vokabel_synonym.Focus();

                    if (vokabel_synonym.ReadOnly == false)
                    {
                        vokabel_synonym.Text = vokabel_synonym.Text + Clipboard.GetText();
                    }
                    vokabel_synonym.SelectionStart = vokabel_synonym.TextLength;

                    break;
            }

        }

        //Teilweise richtig vorbereiten

        private string prepare_text(string text)
        {

            //Text bearbeiten

            //Leerschläge

            if (Properties.Settings.Default.nearly_correct_blank_char == true)
            {
                text = text.Replace(" ", "");
            }

            //Satzzeichen

            if (Properties.Settings.Default.nearly_correct_punctuation_char == true)
            {
                text = text.Replace(",", "");
                text = text.Replace(".", "");
                text = text.Replace(";", "");
                text = text.Replace("-", "");
                text = text.Replace("!", "");
                text = text.Replace("?", "");
                text = text.Replace("'", "");
                text = text.Replace("\\", "");
                text = text.Replace("/", "");
                text = text.Replace("(", "");
                text = text.Replace(")", "");
            }

            //Sonderzeichen

            if (Properties.Settings.Default.nearly_correct_special_char == true)
            {
                text = text.Replace("ä", "a");
                text = text.Replace("ö", "o");
                text = text.Replace("ü", "u");
                text = text.Replace("ß", "ss");

                text = text.Replace("à", "a");
                text = text.Replace("â", "a");
                text = text.Replace("ă", "a");
                text = text.Replace("æ", "oe");
                text = text.Replace("ç", "c");
                text = text.Replace("é", "e");
                text = text.Replace("è", "e");
                text = text.Replace("ê", "e");
                text = text.Replace("ë", "e");
                text = text.Replace("ï", "i");
                text = text.Replace("î", "i");
                text = text.Replace("ì", "i");
                text = text.Replace("í", "i");
                text = text.Replace("ñ", "n");
                text = text.Replace("ô", "o");
                text = text.Replace("ò", "o");
                text = text.Replace("ó", "o");
                text = text.Replace("œ", "oe");
                text = text.Replace("ş", "s");
                text = text.Replace("ţ", "t");
                text = text.Replace("ù", "u");
                text = text.Replace("ú", "u");
                text = text.Replace("û", "u");
                text = text.Replace("ÿ", "y");

                text = text.Replace("ª", "");
                text = text.Replace("º", "");
                text = text.Replace("¡", "");
                text = text.Replace("¿", "");

            }

            //Artikel bearbeiten

            if (Properties.Settings.Default.nearly_correct_artical == true)
            {

                //Deutsch

                text = text.Replace("der", "");
                text = text.Replace("die", "");
                text = text.Replace("das", "");
                text = text.Replace("des", "");
                text = text.Replace("dem", "");
                text = text.Replace("den", "");
                text = text.Replace("ein", "");
                text = text.Replace("eine", "");
                text = text.Replace("eines", "");
                text = text.Replace("einer", "");
                text = text.Replace("einem", "");
                text = text.Replace("einen", "");


                //Französisch

                text = text.Replace("un", "");
                text = text.Replace("une", "");
                text = text.Replace("le", "");
                text = text.Replace("la", "");
                text = text.Replace("les", "");
                text = text.Replace("l'", "");

                //Englisch

                text = text.Replace("a", "");
                text = text.Replace("an", "");
                text = text.Replace("that", "");
                text = text.Replace("the", "");

                //Italienisch

                text = text.Replace("il", "");
                text = text.Replace("i", "");
                text = text.Replace("lo", "");
                text = text.Replace("gli", "");
                text = text.Replace("uno", "");
                text = text.Replace("una", "");
                text = text.Replace("un'", "");

                //Spanisch

                text = text.Replace("el", "");
                text = text.Replace("los", "");
                text = text.Replace("las", "");
                text = text.Replace("unos", "");
                text = text.Replace("unas", "");
                text = text.Replace("lo", "");
                text = text.Replace("otro", "");
                text = text.Replace("medio", "");

            }

            text = text.ToUpper();

            return text;
        }


        //Schliessen
        private void practise_dialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            sonderzeichen_dialog.Close();

            mci.Close();
        }
    }
}