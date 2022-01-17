using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Word = Microsoft.Office.Interop.Word;
using System.Threading.Tasks;
using System.Windows;

namespace Проекты_8_9_Классы.res
{
    public static class FSSTFilePresenter
    {

        public enum FsstFileType
        {
            Single,
            Combined
        };


        //  Добавив или удалив поле в классе файла убедитесь, что вы внесли изменения в следующие функции:
        //  LoadSysSettingFileFromBytes
        //  ToBytes
        //  Операторы:
        //  ==
        public class FSSTFile : IEquatable<FSSTFile>
        {
            public char[] magic;
            public UInt64 FileSize;
            public string version;
            public byte CriteriaAmount;
            public Int16 StudentAmount { get; set; } = 0;
            public UInt16[] ExaminationDate;
            public UInt16[] InputDate;

            private StudentStruct.Student[] _students;
            public StudentStruct.Student[] students
            {
                get => _students;
                set
                {
                    _students = value;
                    StudentAmount = Convert.ToInt16(students.Length);
                }
            }
            public string HeadTeacherName;
            public UInt16 TeacherAmount;
            public List<string> TeacherNames;
            public FsstFileType FileType;

            public FSSTFile(FsstFileType fileType = FsstFileType.Single, byte criteriaAmount = 14, StudentStruct.Student[] students = null, string headTeacherName = "", List<string> teacherNames = null,  string examinationDate = "", string inputDate = "")
            {
                version = "v2.0";
                magic = new char[] { 'F', 'S', 'S', 'T' };
                FileType = fileType;

                StudentAmount = 0;
                CriteriaAmount = criteriaAmount;

                #region exam
                ExaminationDate = new UInt16[3];
                string[] _exam = examinationDate.Split('.').ToArray();

                if (!(_exam.Length == 3 && UInt16.TryParse(_exam[0], out ExaminationDate[0])
                                      && UInt16.TryParse(_exam[1], out ExaminationDate[1])
                                      && UInt16.TryParse(_exam[2], out ExaminationDate[2])))
                {
                    ExaminationDate = new ushort[3];
                }
                #endregion
                #region input
                InputDate = new UInt16[3];
                string[] _input = inputDate.Split('.').ToArray();
                if (!(_input.Length == 3 && UInt16.TryParse(_input[0], out InputDate[0])
                                      && UInt16.TryParse(_input[1], out InputDate[1])
                                      && UInt16.TryParse(_input[2], out InputDate[2])))
                {
                    InputDate = new ushort[3];
                }
                #endregion

                HeadTeacherName = headTeacherName;

                TeacherNames = new List<string>();
                if (teacherNames != null)
                {
                    TeacherNames = teacherNames;
                    TeacherAmount = UInt16.Parse(teacherNames.Count.ToString());
                }

                this.students = new StudentStruct.Student[0];
                if (students != null)
                {
                    this.students = students;
                    StudentAmount = Int16.Parse(students.Length.ToString());
                }

            }

            public static bool operator ==(FSSTFile a, FSSTFile b)
            {
                if (ReferenceEquals(a, b))
                    return true;

                if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                    return false;

                var a1 = FSSTFilePresenter.FSSTFileToBytes(a);
                var a2 = FSSTFilePresenter.FSSTFileToBytes(b);

                return a1.Except(a2).Count() == 0;
            }

            public static bool operator !=(FSSTFile a, FSSTFile b) => !(a == b);

            public static void SetStudentAmount(FSSTFile file, Int16 count) => file.StudentAmount = count;

            public bool Equals(FSSTFile other)
            {
                if (object.ReferenceEquals(other, null)) return false;

                if (object.ReferenceEquals(this, other)) return true;

                return this == other;
            }
        }

        public static UInt16 FsstFileTypeToUInt16(FsstFileType FileType)
        {
            switch (FileType)
            {
                case FsstFileType.Single:
                    return 0;
                case FsstFileType.Combined:
                    return 1;
                default:
                    return 0;
            }
        }

        public static FsstFileType UInt16ToFsstFileType(UInt16 code)
        {
            switch (code)
            {
                case 0:
                    return FsstFileType.Single;
                case 1:
                    return FsstFileType.Combined;
                default:
                    return FsstFileType.Single;
            }
        }

        private static string[] LoadFileStrings(string path)
        {
            return File.ReadAllLines(path, Encoding.GetEncoding(1251));
        }

        #region Load Student Data

        private static StudentStruct.Student[] GetStudents(String[] FileStrings)
        {
            List<StudentStruct.Student> students = new List<StudentStruct.Student>();

            for (int i = 0; i < FileStrings.Length; i++)
            {
                StudentStruct.Student student = new StudentStruct.Student();
                int elementNumber = 0;
                string str = "";

                for (int j = 0; j < FileStrings[i].Length; j++)
                {

                    if (FileStrings[i][j] == ';')
                    {
                        switch (elementNumber)
                        {
                            case 0:
                                student.ID = Convert.ToByte(str);
                                break;
                            case 1:
                                student.FIO = str;
                                break;
                            case 2:
                                student.Form = str;
                                break;
                            case 3:
                                student.Theme = str;
                                break;
                            case 4:
                                student.Tutor = str;
                                break;
                            case 5:
                                byte[] arr = new byte[str.Length / 2];
                                for (int index = 1; index < str.Length; index += 2)
                                {
                                    arr[index / 2] = Convert.ToByte(str[index].ToString());
                                }
                                student.IndividualCriteriaScores = arr;
                                break;
                        }
                        elementNumber++;
                        str = "";
                    }
                    else
                    {
                        if (j + 1 == FileStrings[i].Length)
                        {
                            str += FileStrings[i][j];
                            switch (elementNumber)
                            {
                                case 0:
                                    student.ID = Convert.ToByte(str);
                                    break;
                                case 1:
                                    student.FIO = str;
                                    break;
                                case 2:
                                    student.Form = str;
                                    break;
                                case 3:
                                    student.Theme = str;
                                    break;
                                case 4:
                                    student.Tutor = str;
                                    break;
                                case 5:
                                    byte[] arr = new byte[str.Length / 2];
                                    for (int index = 1; index < str.Length; index += 2)
                                    {
                                        arr[index / 2] = Convert.ToByte(str[index].ToString());
                                    }
                                    student.IndividualCriteriaScores = arr;
                                    break;
                            }
                            elementNumber++;
                            str = "";
                        }
                        else
                        {
                            str += FileStrings[i][j];
                        }
                    }

                }

                students.Add(student);

            }

            return students.ToArray();
        }

        public static FSSTFile LoadFile()
        {
            return new FSSTFile();
        }
        #endregion

        public static void WriteToByteFSSTFile(this FSSTFile FsstFile, string fileName)
        {
            byte[] testFile = FSSTFileToBytes(FsstFile);
            File.WriteAllBytes(fileName, testFile);

        }

        public static byte[] FSSTFileToBytes(FSSTFile file)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memoryStream);

            bw.Write(file.magic);
            bw.Write(file.FileSize);
            bw.Write(file.version);

            bw.Write(file.ExaminationDate[0]);
            bw.Write(file.ExaminationDate[1]);
            bw.Write(file.ExaminationDate[2]);

            bw.Write(file.InputDate[0]);
            bw.Write(file.InputDate[1]);
            bw.Write(file.InputDate[2]);

            bw.Write(file.CriteriaAmount);
            bw.Write(file.StudentAmount);

            foreach (StudentStruct.Student student in (file.StudentAmount != 0) ? file.students : new StudentStruct.Student[0])
            {
                bw.Write(student.ID);
                bw.Write(student.FIO);
                bw.Write(student.Form);
                bw.Write(student.Theme);
                bw.Write(student.Tutor);
                bw.Write(student.IndividualCriteriaScores);
                bw.Write(student.LastEditDate);

            }

            bw.Write(file.HeadTeacherName);

            bw.Write(file.TeacherAmount);

            foreach (var item in file.TeacherNames)
            {
                bw.Write(item);
            }

            bw.Write(FsstFileTypeToUInt16(file.FileType));

            return memoryStream.ToArray();

        }



        public static FSSTFile BytesToFSSTFile(byte[] storedData)
        {
            FSSTFile file = new FSSTFile();

            MemoryStream memoryStream = new MemoryStream(storedData);
            BinaryReader br = new BinaryReader(memoryStream);

            try
            {

                file.magic = br.ReadChars(4);
                if (file.magic.Except(new char[] { 'F','S','S','T'}).Count() != 0)
                {
                    throw new Exception();
                }

                file.FileSize = br.ReadUInt64();
                file.version = br.ReadString();

                file.ExaminationDate = new UInt16[3];
                file.ExaminationDate[0] = br.ReadUInt16();
                file.ExaminationDate[1] = br.ReadUInt16();
                file.ExaminationDate[2] = br.ReadUInt16();

                file.InputDate = new UInt16[3];
                file.InputDate[0] = br.ReadUInt16();
                file.InputDate[1] = br.ReadUInt16();
                file.InputDate[2] = br.ReadUInt16();

                file.CriteriaAmount = br.ReadByte();
                file.StudentAmount = br.ReadInt16();

                List<StudentStruct.Student> students = new List<StudentStruct.Student>();

                for (int i = 0; i < file.StudentAmount; i++)
                {
                    StudentStruct.Student student = new StudentStruct.Student();

                    student.ID = br.ReadByte();
                    student.FIO = br.ReadString();
                    student.Form = br.ReadString();
                    student.Theme = br.ReadString();
                    student.Tutor = br.ReadString();
                    student.IndividualCriteriaScores = br.ReadBytes(file.CriteriaAmount);
                    student.LastEditDate = br.ReadString();

                    students.Add(student);
                }

                file.students = students.ToArray();
            }
            catch (Exception ex)
            {
                file = new FSSTFile();
                MessageBox.Show("Невозможно открыть выбраный файл.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            try
            {
                file.HeadTeacherName = br.ReadString();
                file.TeacherAmount = br.ReadUInt16();
                file.TeacherNames = new List<string>();

                for (int i = 0; i < file.TeacherAmount; i++)
                {
                    file.TeacherNames.Add(br.ReadString());
                }

            }
            catch (Exception)
            {
                file.HeadTeacherName = "";
                file.TeacherAmount = 0;
                file.TeacherNames = new List<string>();
            }

            try
            {
                file.FileType = UInt16ToFsstFileType(br.ReadUInt16());
            }
            catch (Exception)
            {
                file.FileType = FsstFileType.Single;
            }

            return file;

        }

        public static void FSSTFileToDocxBase(this FSSTFile FsstFile, string fileName, string[] examinationDate, string[] inputDate)
        {
            string template = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\base.docx";

            

            Word.Document TempDoc = (Application.Current as App).wordApp.Documents.OpenNoRepairDialog(template);
            TempDoc.Activate();
            Word.Table table;
            try
            {
                table = TempDoc.Tables[1];
            }
            catch (Exception)
            {
                MessageBox.Show("Файл \"" + template + "\" не соответствует требуемому шаблону", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TempDoc.Close();
                return;
            };


            int p = table.Rows.Count - 1 - FsstFile.StudentAmount;

            if (p > 0)
            {
                for (int i = 0; i < p; i++)
                {
                    table.Rows[FsstFile.StudentAmount + 2].Delete();
                }

            }
            else if (p < 0)
            {
                for (int i = 0; i < -p; i++)
                {
                    table.Rows.Add();
                }
            }

            for (int i = 2; i < FsstFile.StudentAmount + 2; i++)
            {
                table.Cell(i, 1).Range.Text = (i - 1).ToString();
                table.Cell(i, 2).Range.Text = FsstFile.students[i - 2].FIO;
                table.Cell(i, 3).Range.Text = FsstFile.students[i - 2].Form;
                table.Cell(i, 4).Range.Text = FsstFile.students[i - 2].Theme;
                table.Cell(i, 5).Range.Text = FsstFile.students[i - 2].Tutor;
                table.Cell(i, 6).Range.Text = FsstFile.students[i - 2].IndividualCriteriaScores.SumBytes().ToString();
                table.Cell(i, 7).Range.Text = StudentStruct.CalculateMark(FsstFile.students[i - 2].IndividualCriteriaScores.SumBytes()).ToString();

            }

            int pIndex = 0;

            for (int i = 0; i < TempDoc.Paragraphs.Count; i++)
            {
                if (TempDoc.Paragraphs[i + 1].Range.Text.Trim().Contains("Дата проведения защиты"))
                {
                    pIndex = i + 1;
                    break;
                }
            }

            try
            {
                TempDoc.Paragraphs[pIndex].Range.Text = new StringBuilder().Append(TempDoc.Paragraphs[pIndex].Range.Text.TakeWhile((x) => x != '«').ToArray()).ToString()
                                                     + "« " + examinationDate[0] + " »\t" + examinationDate[1] + " " + examinationDate[2] + "г. \n";

                if (!TempDoc.Paragraphs[pIndex + 1].Range.Text.Contains("Дата внесения в протокол оценок"))
                {
                    throw new Exception();
                }
                TempDoc.Paragraphs[pIndex + 1].Range.Text = new StringBuilder().Append(TempDoc.Paragraphs[pIndex + 1].Range.Text.TakeWhile((x) => x != '«').ToArray()).ToString()
                                                         + "« " + inputDate[0] + " »\t" + inputDate[1] + " " + inputDate[2] + "г. \n";
            }
            catch (Exception)
            {
                MessageBox.Show("Файл \"" + template + "\" не соответствует требуемому шаблону", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TempDoc.Close();
                return;
            };


            TempDoc.SaveAs2(fileName);
            TempDoc.Close();

            MessageBox.Show("Файл \"" + new StringBuilder().Append(fileName.Reverse().TakeWhile((x) => x != '\\').Reverse().ToArray()).ToString() + "\" был успешно сохранен",
                "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        public static void FSSTFileToNewDocx(this FSSTFile FsstFile, string fileName)
        {
            string[] examinationDate = (Application.Current.MainWindow as MainWindow).DateToStrings(FsstFile.ExaminationDate);
            string[] inputDate = (Application.Current.MainWindow as MainWindow).DateToStrings(FsstFile.InputDate);
            Word.Application App = new Word.Application();

            Word.Document newDoc = App.Documents.Add();
            Word.ParagraphFormat format = new Word.ParagraphFormat();

            newDoc.PageSetup.TopMargin = 30;
            newDoc.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            Word.Paragraph pr1 = newDoc.Paragraphs.Add(); pr1.Range.Text = "ПРОТОКОЛ\r\n";
            Word.Paragraph pr2 = newDoc.Paragraphs.Add(); pr2.Range.Text = "защиты итогового индивидуального проекта\r\n";
            Word.Paragraph pr3 = newDoc.Paragraphs.Add(); pr3.Range.Text = "обучающимися муниципального автономного общеобразовательного\r\n";
            Word.Paragraph pr4 = newDoc.Paragraphs.Add(); pr4.Range.Text = "учреждения г. Хабаровска\r\n";
            Word.Paragraph pr5 = newDoc.Paragraphs.Add(); pr5.Range.Text = "«Лицей инновационных технологий»\r\n";

            newDoc.Paragraphs.Add();

            pr5.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;


            Word.Table table = newDoc.Tables.Add(newDoc.Paragraphs.Add().Range, FsstFile.StudentAmount + 1, 7, System.Reflection.Missing.Value, Word.WdAutoFitBehavior.wdAutoFitContent);
            table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

            table.Columns[1].Width = 30f;
            table.Columns[2].Width = 90f;
            table.Columns[3].Width = 45f;
            table.Columns[4].Width = 90f;
            table.Columns[5].Width = 90f;
            table.Columns[6].Width = 80f;
            table.Columns[7].Width = 50f;

            table.Cell(1, 1).Range.Text = "№\r\nп/п"; table.Cell(1, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(1, 2).Range.Text = "ФИО обучающегося"; table.Cell(1, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(1, 3).Range.Text = "Класс"; table.Cell(1, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(1, 4).Range.Text = "Тема проекта"; table.Cell(1, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(1, 5).Range.Text = "Руководитель проекта"; table.Cell(1, 5).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(1, 6).Range.Text = "Количество баллов"; table.Cell(1, 6).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            table.Cell(1, 7).Range.Text = "Оценка"; table.Cell(1, 7).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            for (int i = 2; i < FsstFile.StudentAmount + 2; i++)
            {
                table.Rows[i].Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                table.Cell(i, 1).Range.Text = (i - 1).ToString();
                table.Cell(i, 2).Range.Text = FsstFile.students[i - 2].FIO;
                table.Cell(i, 3).Range.Text = FsstFile.students[i - 2].Form;
                table.Cell(i, 4).Range.Text = FsstFile.students[i - 2].Theme;
                table.Cell(i, 5).Range.Text = FsstFile.students[i - 2].Tutor;
                table.Cell(i, 6).Range.Text = FsstFile.students[i - 2].IndividualCriteriaScores.SumBytes().ToString();
                table.Cell(i, 7).Range.Text = StudentStruct.CalculateMark(FsstFile.students[i - 2].IndividualCriteriaScores.SumBytes()).ToString();

                table.Cell(i, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                table.Cell(i, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                table.Cell(i, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                table.Cell(i, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                table.Cell(i, 5).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                table.Cell(i, 6).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                table.Cell(i, 7).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            }

            newDoc.Paragraphs.Add();

            Word.Paragraph date_1 = newDoc.Paragraphs.Add();
            date_1.Range.Text = "Дата проведения защиты:                « " + examinationDate[0] + " »\t" + examinationDate[1] + " " + examinationDate[2] + "г. \r\n";
            Word.Paragraph date_2 = newDoc.Paragraphs.Add();
            date_2.Range.Text = "Дата внесения в протокол оценок:  « " + inputDate[0] + " »\t" + inputDate[1] + " " + inputDate[2] + "г. \r\n";

            Word.Paragraph pr6 = newDoc.Paragraphs.Add();
            pr6.Range.Text = "Председатель комиссии:         ____________________/  " + FsstFile.HeadTeacherName + "\r\n";
            pr6.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            pr6.Range.Font.Superscript = 1;

            Word.Paragraph sign_1 = newDoc.Paragraphs.Add();
            sign_1.Range.Text = "                          (подпись)                         (расшифровка)\r\n"; sign_1.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            sign_1.Range.Font.Superscript = 0;

            Word.Paragraph pr7 = newDoc.Paragraphs.Add();
            pr7.Range.Text = "Члены комиссии:                     ____________________/  " + FsstFile.TeacherNames[0] + "\r\n";
            pr7.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            pr7.Range.Font.Superscript = 1;

            Word.Paragraph sign_2 = newDoc.Paragraphs.Add();
            sign_2.Range.Text = "                          (подпись)                         (расшифровка)\r\n";
            sign_2.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            sign_2.Range.Font.Superscript = 0;

            for (int i = 1; i < FsstFile.TeacherNames.Count; i++)
            {
                Word.Paragraph pr = newDoc.Paragraphs.Add();
                pr.Range.Text = " ".Multiply(29) + "                     ____________________/  " + FsstFile.TeacherNames[i] + "\r\n";
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
                pr.Range.Font.Size = 12;
                pr.SpaceAfter = 0;

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

    }
}
