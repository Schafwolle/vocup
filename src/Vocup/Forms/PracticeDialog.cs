using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Vocup.Forms;
using Vocup.Models;
using Vocup.Properties;

namespace Vocup.Forms
{
    public partial class PracticeDialog : Form
    {
        public int anz_vok;
        public int anz_geübt;
        public int anz_noch_nicht;
        public int anz_richtig = 0;
        public int anz_falsch = 0;
        public int anz_teilweise = 0;

        public string[,] practise_list;

        private VocabularyBook book;
        private List<VocabularyWordPractice> practiceList;
        private VocabularyWordPractice currentPractice;
        private VocabularyWord currentWord;
        private int index;
        private SpecialCharKeyboard specialCharDialog;

        private bool check;
        private bool solution;

        // TODO: Use System.Media.SoundPlayer instead of MCIPlayback
        private SimpleAudioVideoPlayback.MCIPlayback mci = new SimpleAudioVideoPlayback.MCIPlayback();

        public PracticeDialog(VocabularyBook book, List<VocabularyWordPractice> practiceList)
        {
            InitializeComponent();

            Icon = Icon.FromHandle(Icons.practise.GetHicon());

            this.book = book;
            this.practiceList = practiceList;
            index = 0;
            specialCharDialog = new SpecialCharKeyboard();
            specialCharDialog.Initialize(this);
            specialCharDialog.VisibleChanged += (a0, a1) => BtnSpecialChar.Enabled = !specialCharDialog.Visible;

            if (Settings.Default.UserEvaluates)
            {
                Height = 370; // All other controls are adjusted by anchor
            }

            LbMotherTongue.Text = book.MotherTongue;
            LbForeignLang.Text = book.ForeignLang;

            if (book.PracticeMode == PracticeMode.AskForForeignLang)
                GroupPractice.Text = string.Format(GroupPractice.Text, book.MotherTongue, book.ForeignLang);
            else
                GroupPractice.Text = string.Format(GroupPractice.Text, book.ForeignLang, book.MotherTongue);
        }

        private void practise_dialog_Load(object sender, EventArgs e)
        {
            TbPracticeCount.Text = Convert.ToString(anz_vok);
            TbPracticedCount.Text = "0";
            TbUnpracticedCount.Text = TbPracticeCount.Text;
            anzahl_richtig.Text = "0";
            anzahl_teilweise.Text = "0";
            anzahl_falsch.Text = "0";

            PbPracticeProgress.Maximum = anz_vok;

            if (Properties.Settings.Default.UserEvaluates)
            {
                GroupUserEvaluation.Visible = true;
                GroupUserEvaluation.Enabled = false;
            }

            //Erste Vokabel einlesen

            set_vokabel();
        }

        private void fortfahren_button_Click(object sender, EventArgs e)
        {
            if (anz_geübt == anz_vok)
            {
                mci.Close();
                Close();
                return;
            }

            if (Settings.Default.UserEvaluates)
            {
                if (check == false)
                {
                    set_vokabel();
                }
                else if (check == true && solution == true)
                {
                    ShowSolution();
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
            else if (Properties.Settings.Default.only_one_click && !Properties.Settings.Default.UserEvaluates)
            {
                check_vokabel();
                if (anz_vok != anz_geübt)
                {
                    set_vokabel();
                }
            }
            else if (check) //Falls die Eingabe überprüft werden soll
            {
                check_vokabel();
            }
            else //Falls die nächste vokabel eingelesen werden soll
            {
                set_vokabel();
            }
        }

        //Eine Vokabel lernen

        public void set_vokabel()
        {
            //Vokabel einlesen

            TbForeignLang.Text = "";
            TbMotherTongue.Text = "";
            TbForeignLangSynonym.Text = "";

            if (Settings.Default.UserEvaluates)
            {
                GroupUserEvaluation.Enabled = false;
            }

            if (!Settings.Default.only_one_click || Settings.Default.UserEvaluates)
            {
                TbCorrectAnswer.Text = "";
                TbCorrectAnswer.BackColor = SystemColors.Control;
            }

            if (book.PracticeMode == PracticeMode.AskForForeignLang)
            {
                TbMotherTongue.Text = currentWord.MotherTongue;
                TbMotherTongue.ReadOnly = true;
                TbForeignLang.ReadOnly = false;
                TbForeignLang.BackColor = Settings.Default.PracticeInputBackColor;
                if (string.IsNullOrWhiteSpace(currentWord.ForeignLangSynonym))
                {
                    TbForeignLangSynonym.BackColor = DefaultBackColor;
                    TbForeignLangSynonym.ReadOnly = true;
                }
                else
                {
                    TbForeignLangSynonym.ReadOnly = false;
                    TbForeignLangSynonym.BackColor = Settings.Default.PracticeInputBackColor;
                }
            }

            if (book.PracticeMode == PracticeMode.AskForMotherTongue)
            {
                TbForeignLang.Text = practise_list[anz_geübt, 3];

                TbMotherTongue.Select();

                TbForeignLangSynonym.Text = practise_list[anz_geübt, 4];

                TbMotherTongue.ReadOnly = false;
                TbForeignLang.ReadOnly = true;
                TbForeignLangSynonym.ReadOnly = true;

                //Eingabe-Felder mit Farbe hervorheben

                if (Properties.Settings.Default.colored_textfields == true)
                {
                    TbMotherTongue.BackColor = Color.FromArgb(250, 250, 150);
                }

                TbForeignLang.BackColor = DefaultBackColor;
                TbForeignLangSynonym.BackColor = DefaultBackColor;
            }
            else //Falls von Muttersprache nach Fremdsprache geübt werden soll
            {
                TbMotherTongue.Text = practise_list[anz_geübt, 2];

                if (practise_list[anz_geübt, 4] == "" || practise_list[anz_geübt, 4] == null)
                {
                    TbForeignLangSynonym.ReadOnly = true;

                    TbForeignLangSynonym.BackColor = DefaultBackColor;
                }
                else
                {
                    TbForeignLangSynonym.ReadOnly = false;

                    if (Properties.Settings.Default.colored_textfields == true)
                    {
                        TbForeignLangSynonym.BackColor = Color.FromArgb(250, 250, 150);
                    }
                }

                TbMotherTongue.ReadOnly = true;
                TbMotherTongue.BackColor = DefaultBackColor;
                TbForeignLang.ReadOnly = false;
                if (Properties.Settings.Default.colored_textfields == true)
                {
                    TbForeignLang.BackColor = Color.FromArgb(250, 250, 150);
                }
                TbForeignLang.Select();
            }

            check = true;
            solution = true;
        }

        //Eingabe kontrollieren
        private void check_vokabel()
        {

            TbForeignLang.ReadOnly = true;
            TbMotherTongue.ReadOnly = true;
            TbForeignLangSynonym.ReadOnly = true;

            GroupUserEvaluation.Enabled = true;

            //Eingabe auf Richtigkeit überprüfen

            bool[] correct = new bool[2];

            if (Properties.Settings.Default.UserEvaluates == false)
            {
                if (book.PracticeMode == PracticeMode.AskForMotherTongue)
                {

                    string vokabel_own_language_komp = prepare_text(TbMotherTongue.Text);
                    string vokabel_own_language_komp_richtig = prepare_text(practise_list[anz_geübt, 2]);

                    //Richtig
                    if (TbMotherTongue.Text == practise_list[anz_geübt, 2] || TbMotherTongue.Text == practise_list[anz_geübt, 2] + " ")
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
                            if (TbMotherTongue.Text.Contains(keywords[i]) == true)
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
                    else //Falsch
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
                        string vokabel_foreign_language_komp = prepare_text(TbForeignLang.Text);


                        //Richtig
                        if (TbForeignLang.Text == practise_list[anz_geübt, 3] || TbForeignLang.Text == practise_list[anz_geübt, 3] + " ")
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
                                if (TbForeignLang.Text.Contains(keywords[i]) == true)
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
                        string vokabel_foreign_language_komp = prepare_text(TbForeignLang.Text);
                        string vokabel_synonym_komp = prepare_text(TbForeignLang.Text);
                        string vokabel_synonym_komp_richtig = prepare_text(practise_list[anz_geübt, 4]);

                        //Richtig
                        if (TbForeignLang.Text == practise_list[anz_geübt, 3] && TbForeignLangSynonym.Text == practise_list[anz_geübt, 4])
                        {
                            correct[0] = true;
                            correct[1] = true;
                        }
                        else if (TbForeignLang.Text == practise_list[anz_geübt, 4] && TbForeignLangSynonym.Text == practise_list[anz_geübt, 3])
                        {
                            correct[0] = true;
                            correct[1] = true;
                        }

                        //Teilweise richtig

                        else if (Properties.Settings.Default.nearly_correct_synonym == true)
                        {

                            if (TbForeignLang.Text == practise_list[anz_geübt, 3] && TbForeignLangSynonym.Text != practise_list[anz_geübt, 4])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                            else if (TbForeignLang.Text == practise_list[anz_geübt, 4] && TbForeignLangSynonym.Text != practise_list[anz_geübt, 3])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                            else if (TbForeignLang.Text != practise_list[anz_geübt, 3] && TbForeignLangSynonym.Text == practise_list[anz_geübt, 4])
                            {
                                correct[0] = true;
                                correct[1] = false;
                            }
                            else if (TbForeignLang.Text != practise_list[anz_geübt, 4] && TbForeignLangSynonym.Text == practise_list[anz_geübt, 3])
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
                                if (TbForeignLang.Text.Contains(keywords[i]) == true)
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
                        else //falsch
                        {
                            correct[0] = false;
                            correct[1] = false;
                        }
                    }
                }
            }
            else //Falls selber bewertet werden soll
            {
                if (RbCorrect.Checked == true)
                {
                    correct[0] = true;
                    correct[1] = true;
                }
                else if (RbPartlyCorrect.Checked == true)
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
                if (!Settings.Default.UserEvaluates)
                {
                    TbCorrectAnswer.Text = Words.Correct + "!";
                    TbCorrectAnswer.BackColor = Color.FromArgb(144, 238, 144);
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

                if (Properties.Settings.Default.UserEvaluates == false)
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
                if (book.PracticeMode == PracticeMode.AskForMotherTongue)
                {
                    if (!Settings.Default.UserEvaluates)
                    {
                        TbCorrectAnswer.Text = $"{Words.PartlyCorrect}! ({string.Format(Words.CorrectWasX, practise_list[anz_geübt, 2])})";
                    }

                    //Ergebnisse speichern
                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "nearly right";
                    practise_list[anz_geübt, 7] = TbMotherTongue.Text;
                }
                else
                {
                    if (!Settings.Default.UserEvaluates)
                    {
                        if (string.IsNullOrWhiteSpace(practise_list[anz_geübt, 4]))
                        {
                            TbCorrectAnswer.Text = $"{Words.PartlyCorrect}! ({string.Format(Words.CorrectWasX, practise_list[anz_geübt, 3])})";
                        }
                        else
                        {
                            TbCorrectAnswer.Text = $"{Words.PartlyCorrect}! ({string.Format(Words.CorrectWasXAndY, practise_list[anz_geübt, 3], practise_list[anz_geübt, 4])})";
                        }
                    }

                    //Ergebnisse speichern
                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "nearly right";
                    practise_list[anz_geübt, 7] = TbForeignLang.Text;
                    if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null && TbForeignLangSynonym.Text != "")
                    {
                        practise_list[anz_geübt, 7] = practise_list[anz_geübt, 7] + ", " + TbForeignLangSynonym.Text;
                    }
                }

                anz_teilweise++;

                if (Properties.Settings.Default.UserEvaluates == false)
                {
                    TbCorrectAnswer.BackColor = Color.FromArgb(255, 215, 0);

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
                if (book.PracticeMode == PracticeMode.AskForMotherTongue)
                {
                    if (!Settings.Default.UserEvaluates)
                    {
                        TbCorrectAnswer.Text = $"{Words.Wrong}! ({string.Format(Words.CorrectWasX, practise_list[anz_geübt, 2])})";
                    }
                    //Ergebnisse speichern

                    practise_list[anz_geübt, 1] = "1";
                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "false";
                    practise_list[anz_geübt, 7] = TbMotherTongue.Text;
                }

                else
                {
                    if (!Settings.Default.UserEvaluates)
                    {
                        if (string.IsNullOrEmpty(practise_list[anz_geübt, 4]))
                        {
                            TbCorrectAnswer.Text = $"{Words.Wrong}! ({string.Format(Words.CorrectWasX, practise_list[anz_geübt, 3])})";
                        }
                        else
                        {
                            TbCorrectAnswer.Text = $"{Words.Wrong}! ({string.Format(Words.CorrectWasXAndY, practise_list[anz_geübt, 3], practise_list[anz_geübt, 4])})";
                        }
                    }
                    //Ergebnisse speichern

                    practise_list[anz_geübt, 1] = "1";
                    practise_list[anz_geübt, 5] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    practise_list[anz_geübt, 6] = "false";
                    practise_list[anz_geübt, 7] = TbForeignLang.Text;
                    if (practise_list[anz_geübt, 4] != "" && practise_list[anz_geübt, 4] != null & TbForeignLangSynonym.Text != "")
                    {
                        practise_list[anz_geübt, 7] = practise_list[anz_geübt, 7] + ", " + TbForeignLangSynonym.Text;
                    }
                }
                anz_falsch++;

                if (Properties.Settings.Default.UserEvaluates == false)
                {
                    TbCorrectAnswer.BackColor = Color.FromArgb(255, 192, 203);

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
                BtnContinue.Text = Words.Finish;
            }
        }

        private void ShowSolution()
        {
            if (book.PracticeMode == PracticeMode.AskForForeignLang)
            {
                if (string.IsNullOrWhiteSpace(currentWord.ForeignLangSynonym))
                    TbCorrectAnswer.Text = string.Format(Words.CorrectWasX, currentWord.ForeignLang);
                else
                    TbCorrectAnswer.Text = string.Format(Words.CorrectWasXAndY, currentWord.ForeignLang, currentWord.ForeignLangSynonym);
            }
            else
            {
                TbCorrectAnswer.Text = string.Format(Words.CorrectWasX, currentWord.MotherTongue);
            }

            TbCorrectAnswer.BackColor = Color.White;

            TbForeignLang.ReadOnly = true;
            TbMotherTongue.ReadOnly = true;
            TbForeignLangSynonym.ReadOnly = true;

            //Radio-Buttons aktivieren

            GroupUserEvaluation.Enabled = true;

            check = true;
            solution = false;
        }

        //Zahlen aktualisieren

        private void refresh_numbers()
        {
            //Progress-Bar

            PbPracticeProgress.Value = anz_geübt;

            //Zahlen aktualiseren

            TbPracticedCount.Text = Convert.ToString(anz_geübt);
            TbUnpracticedCount.Text = Convert.ToString(anz_vok - anz_geübt);
            anzahl_richtig.Text = Convert.ToString(anz_richtig);
            anzahl_teilweise.Text = Convert.ToString(anz_teilweise);
            anzahl_falsch.Text = Convert.ToString(anz_falsch);

        }

        //Sonderzeichen
        private void TextBox_Enter(object sender, EventArgs e)
        {
            specialCharDialog.RegisterTextBox((TextBox)sender);
        }

        private void sonderzeichen_button_Click(object sender, EventArgs e)
        {
            specialCharDialog.Show();
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
        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            mci.Close();
        }
    }
}