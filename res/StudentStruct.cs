using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Проекты_8_9_Классы;
using Word = Microsoft.Office.Interop.Word;

namespace Проекты_8_9_Классы.res
{
    public static class StudentStruct
    {

        public class Student
        {
            public byte ID { get; set; }
            public string FIO { get; set; }
            public string Form { get; set; }
            public string Theme { get; set; }
            public string Tutor { get; set; }
            public byte[] IndividualCriteriaScores { get; set; }
            public string LastEditDate { get; set; }

            public readonly UInt16 StudentDataSize;

            public Student(byte ID, string FIO, string Form, string Theme, string Tutor, byte[] scores, string date)
            {
                this.ID = ID;
                this.FIO = FIO;
                this.Form = Form;
                this.Theme = Theme;
                this.Tutor = Tutor;
                this.LastEditDate = date;
                this.IndividualCriteriaScores = scores;

                this.StudentDataSize = (UInt16)(1 + FIO.Length + 1 + Form.Length + 1 + Theme.Length + 1 + Tutor.Length + 1 + 1 + 1 + IndividualCriteriaScores.Length * 1 + date.Length + 1);

            }
            public Student()
            {
                ID = 0;
                FIO = null;
                Form = null;
                Theme = null;
                Tutor = null;
                LastEditDate = DateTime.Today.ToString();

                IndividualCriteriaScores = new byte[GetCriteriaAmount()];

                StudentDataSize = (UInt16)(1 + 1 + 1 + +IndividualCriteriaScores.Length * 1 + LastEditDate.Length + 1);
            }

            public static bool operator ==(Student first, Student second) => first.Equals(second);
            public static bool operator !=(Student first, Student second) => !first.Equals(second);

            public override bool Equals(object obj)
            {   
                if (obj == null || this == null)
                    return false;

                if (obj is Student)
                    return this.FIO.Trim(' ', '.', ',') == (obj as Student).FIO.Trim(' ', '.', ',') &&
                        this.Form.Trim(' ', '.', ',') == (obj as Student).Form.Trim(' ', '.', ',');

                return false;
            }
            public override int GetHashCode()
            {
                return (FIO + Form).GetHashCode();
            }

        }

        public static bool IsStudentArray_Contains(Student[] students, Student student)
        {
            foreach (var collStud in students)
            {
                if (collStud == student)
                    return true;
            }

            return false;
        }

        public static int StudentArray_IndexOf (Student[] students, Student student)
        {
            for (int i = 0; i < students.Length; i++)
            {
                if (students[i].Equals(student))
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        public static void studentDataToIndivProtocol(this Student student, string fileName)
        {
            Word.Application App = new Word.Application();

            Word.Document newDoc = App.Documents.Add();
            Word.ParagraphFormat format = new Word.ParagraphFormat();

            newDoc.PageSetup.TopMargin = 30;
            newDoc.PageSetup.LeftMargin = 40;
            newDoc.PageSetup.BottomMargin = 0;

            Word.Paragraph pr0 = newDoc.Paragraphs.Add();
            pr0.Range.Text = " ";
            pr0.Range.Bold = 1;
            pr0.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            Word.Paragraph pr1 = newDoc.Paragraphs.Add();
            pr1.Range.Text = "Таблица оценивания индивидуального проекта учащегося\r\n";
            pr1.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;

            Word.Paragraph pr2 = newDoc.Paragraphs.Add();
            //42
            pr2.Range.Text = "ФИО: \r\n";
            pr2.Range.Bold = 0;

            Word.Paragraph pr2_1 = newDoc.Paragraphs.Add();
            pr2_1.Range.Text = "\t" + student.FIO + "\r\n";
            pr2_1.Range.Bold = 1;

            Word.Paragraph pr3 = newDoc.Paragraphs.Add();
            pr3.Range.Text = "Образовательное учреждение:\r\n";
            pr3.Range.Bold = 0;

            Word.Paragraph pr3_1 = newDoc.Paragraphs.Add();
            pr3_1.Range.Text = "\tМуниципальное автономное общеобразовательное учреждение г. Хабаровска\r\n";

            Word.Paragraph pr3_2 = newDoc.Paragraphs.Add();
            pr3_2.Range.Text = "\t«Лицей инновационных технологий»\r\n";
            pr3_2.Range.Bold = 1;

            Word.Paragraph pr4_0 = newDoc.Paragraphs.Add();
            pr4_0.Range.Text = "Тема проекта:\r\n";
            pr4_0.Range.Bold = 0;

            Word.Paragraph pr4_1 = newDoc.Paragraphs.Add();
            pr4_1.Range.Text = "\t" + student.Theme + "\r\n";
            pr4_1.Range.Bold = 1;

            Word.Paragraph pr5 = newDoc.Paragraphs.Add();
            pr5.Range.Text = "Руководитель  проекта:\r\n";
            pr5.Range.Bold = 0;

            Word.Paragraph pr5_1 = newDoc.Paragraphs.Add();
            pr5_1.Range.Text = "\t" + student.Tutor + "\r\n";

            #region Table

            Word.Table table = newDoc.Tables.Add(newDoc.Paragraphs.Add().Range, 20, 2, System.Reflection.Missing.Value, Word.WdAutoFitBehavior.wdAutoFitContent);
            table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

            table.Columns[1].Width = 450f;
            table.Columns[2].Width = 70f;

            table.Cell(1, 1).Range.Text = "Критерии оценивания";
            table.Cell(1, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            table.Cell(1, 2).Range.Text = "Баллы\r\n1 – 3 балл";

            table.Cell(2, 1).Merge(table.Cell(2, 2));
            table.Cell(2, 1).Range.Text = "1. Способность к самостоятельному приобретению знаний и решению проблем";
            table.Cell(2, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

            table.Cell(3, 1).Range.Text = "1.1. Поиск, отбор и адекватное использование информации";
            table.Cell(3, 2).Range.Text = student.IndividualCriteriaScores[0].ToString();

            table.Cell(4, 1).Range.Text = "1.2. Актуальность и значимость темы проекта";
            table.Cell(4, 2).Range.Text = student.IndividualCriteriaScores[1].ToString();

            table.Cell(5, 1).Range.Text = "1.3. Личная заинтересованность автора, творческий подход к работе";
            table.Cell(5, 2).Range.Text = student.IndividualCriteriaScores[2].ToString();

            table.Cell(6, 1).Range.Text = "1.4. Полезность и востребованность продукта";
            table.Cell(6, 2).Range.Text = student.IndividualCriteriaScores[3].ToString();

            table.Cell(7, 1).Merge(table.Cell(7, 2));
            table.Cell(7, 1).Range.Text = "2. Сформированность предметных знаний и способов действий";
            table.Cell(7, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

            table.Cell(8, 1).Range.Text = "2.1. Соответствие выбранных способов работы цели и содержанию проекта ";
            table.Cell(8, 2).Range.Text = student.IndividualCriteriaScores[4].ToString();

            table.Cell(9, 1).Range.Text = "2.2. Глубина раскрытия темы проекта ";
            table.Cell(9, 2).Range.Text = student.IndividualCriteriaScores[5].ToString();

            table.Cell(10, 1).Range.Text = "2.3.  Качество проектного продукта ";
            table.Cell(10, 2).Range.Text = student.IndividualCriteriaScores[6].ToString();

            table.Cell(11, 1).Range.Text = "2.4. Использование средств наглядности, технических средств ";
            table.Cell(11, 2).Range.Text = student.IndividualCriteriaScores[7].ToString();

            table.Cell(12, 1).Merge(table.Cell(12, 2));
            table.Cell(12, 1).Range.Text = "3. Сформированность регулятивных действий";
            table.Cell(12, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

            table.Cell(13, 1).Range.Text = "3.1. Соответствие требованиям оформления письменной части ";
            table.Cell(13, 2).Range.Text = student.IndividualCriteriaScores[8].ToString();

            table.Cell(14, 1).Range.Text = "3.2.  Постановка цели, планирование путей ее достижения";
            table.Cell(14, 2).Range.Text = student.IndividualCriteriaScores[9].ToString();

            table.Cell(15, 1).Range.Text = "3.3. Сценарий защиты (логика изложения), грамотное построение доклада";
            table.Cell(15, 2).Range.Text = student.IndividualCriteriaScores[10].ToString();

            table.Cell(16, 1).Range.Text = "3.4. Соблюдение регламента защиты и степень воздействия на аудиторию";
            table.Cell(16, 2).Range.Text = student.IndividualCriteriaScores[11].ToString();

            table.Cell(17, 1).Merge(table.Cell(17, 2));
            table.Cell(17, 1).Range.Text = "4. Сформированность коммуникативных действий";
            table.Cell(17, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

            table.Cell(18, 1).Range.Text = "4.1. Четкость и точность, убедительность и лаконичность";
            table.Cell(18, 2).Range.Text = student.IndividualCriteriaScores[12].ToString();

            table.Cell(19, 1).Range.Text = "4.2 Умение отвечать на вопросы, умение защищать свою точку зрения";
            table.Cell(19, 2).Range.Text = student.IndividualCriteriaScores[13].ToString();

            table.Cell(20, 1).Range.Text = "Итого: ";
            table.Cell(20, 1).Range.Bold = 1;
            table.Cell(20, 2).Range.Text = student.IndividualCriteriaScores.SumBytes().ToString();
            table.Cell(20, 2).Range.Bold = 1;



            #endregion

            newDoc.Paragraphs.Add();

            Word.Paragraph date_1 = newDoc.Paragraphs.Add();
            date_1.Range.Text = "Дата: " + student.LastEditDate + "г. \r\n";
            date_1.Range.Bold = 0;

            Word.Paragraph pr7 = newDoc.Paragraphs.Add();
            pr7.Range.Text = "Председатель комиссии:         ____________________/  " + (Application.Current.MainWindow as MainWindow).FsstFile.HeadTeacherName + "\r\n";
            pr7.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            pr7.Range.Font.Superscript = 1;

            Word.Paragraph sign_1 = newDoc.Paragraphs.Add();
            sign_1.Range.Text = "                          (подпись)                         (расшифровка)\r\n"; sign_1.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            sign_1.Range.Font.Superscript = 0;

            Word.Paragraph pr8 = newDoc.Paragraphs.Add();
            pr8.Range.Text = "Члены комиссии:                     ____________________/  " + (Application.Current.MainWindow as MainWindow).FsstFile.TeacherNames[0] + "\r\n";
            pr8.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            pr8.Range.Font.Superscript = 1;

            Word.Paragraph sign_2 = newDoc.Paragraphs.Add();
            sign_2.Range.Text = "                          (подпись)                         (расшифровка)\r\n";
            sign_2.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            sign_2.Range.Font.Superscript = 0;

            for (int i = 1; i < (Application.Current.MainWindow as MainWindow).FsstFile.TeacherNames.Count; i++)
            {
                Word.Paragraph pr = newDoc.Paragraphs.Add();
                pr.Range.Text = " ".Multiply(29) + "                     ____________________/  " + (Application.Current.MainWindow as MainWindow).FsstFile.TeacherNames[i] + "\r\n";
                pr.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                pr.Range.Font.Superscript = 1;

                Word.Paragraph sign = newDoc.Paragraphs.Add();
                sign.Range.Text = "                          (подпись)                         (расшифровка)\r\n";
                sign.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                sign.Range.Font.Superscript = 0;
            }

            foreach (Word.Paragraph pr in newDoc.Paragraphs)
            {
                pr.Range.Font.Name = "Times New Roman";
                pr.Range.Font.Size = 14;
                Console.WriteLine(pr.Parent.ToString());
                pr.SpaceAfter = 8;

            }

            for (int i = 1; i < 21; i++)
            {
                table.Cell(i, 1).Range.ParagraphFormat.SpaceAfter = 1;

                if (!(i == 2 || i == 7 || i == 12 || i == 17))
                {
                    table.Cell(i, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(i, 2).Range.ParagraphFormat.SpaceAfter = 1;
                }

            }

            FileStream fs = null;
            bool isError = false;
            do
            {
                try
                {
                    fs = System.IO.File.Open(fileName, FileMode.Open);
                    isError = false;
                }
                catch (Exception ex)
                {
                    if (ex.IsFileLocked())
                    {
                        string name = String.Join("", fileName.Reverse().TakeWhile((x) => x != '\\').Reverse());
                        // Файл открыт в стороннем процессе
                        if (MessageBox.Show("Операция не можт быть завершена: файл \"" + name + "\" открыт в стороннем приложении. Пожалуйста, закройте этот файл и повторите попытку.",
                                        "\"" + name + "\" открыт", MessageBoxButton.OKCancel, MessageBoxImage.Error)
                                        == MessageBoxResult.Cancel
                                        )
                        {
                            newDoc.Close();
                            App.Quit();
                            return;
                        }

                        isError = true;

                    }
                }
                finally
                {
                    fs?.Close();
                }
            } while (isError);

            newDoc.SaveAs2(fileName);
            newDoc.Close();
            App.Quit();

            MessageBox.Show("Файл \"" + new StringBuilder().Append(fileName.Reverse().TakeWhile((x) => x != '\\').Reverse().ToArray()).ToString() + "\" был успешно сохранен",
               "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);

            (Application.Current.MainWindow as MainWindow).IsExporting = false;
        }

        public static int GetCriteriaAmount()
        {

            var arr = (Application.Current.MainWindow as MainWindow).criteriaTable.ItemBase.Children;

            int count = 0;
            foreach (FrameworkElement el in arr)
            {
                if (el.Tag?.ToString() == "criterion")
                {
                    count++;
                }
            }

            return count;

        }

        public static byte CalculateMark(float score)
        {
            if (score <= 24)
            {
                return 3;
            }
            else
                if (score <= 35)
            {
                return 4;
            }
            else
                return 5;

        }

        public static Student[] IndividualStudentsFromMatrix(this Student[][] students)
        {
            HashSet<Student> res = new HashSet<Student>();
            for (int i = 0; i < students.GetLength(0); i++)
            {
                for (int j = 0; j < students[i].Length; j++)
                {
                    res.Add(students[i][j]);
                }
                
            }
            return res.OrderBy(x => x.FIO).ToArray();

        }

    }
}
