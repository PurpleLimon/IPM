using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace Проекты_8_9_Классы.res
{
    public class SystemSettings
    {
        //  Добавив или удалив поле в классе файла убедитесь, что вы внесли изменения в следующие функции:
        //  LoadSysSettingFileFromBytes
        //  ToBytes
        //  Операторы:
        //  ==
        public class SysSettingsFile
        {
            public bool IsFirstlyOpened { get; set; } = true;
            public string ExamintionDate { get; set; } = DateTime.Today.ToString("dd.MM.yyyy");
            public string InputDate { get; set; } = DateTime.Today.ToString("dd.MM.yyyy");
            public string ChairmanName { get; set; } = "";
            public byte TeachersCount { get; set; } = 1;
            public string[] TeacherNames { get; set; } = new string[] { "" };

            public SysSettingsFile(bool isFirstlyOpened, string chairmanName, byte teachersCount, string[] teacherNames, string examDate, string inpDate)
            {
                IsFirstlyOpened = isFirstlyOpened;
                ChairmanName = chairmanName;
                TeachersCount = teachersCount;
                TeacherNames = teacherNames;
                InputDate = inpDate;
                ExamintionDate = examDate;
            }

            public SysSettingsFile()
            {
                IsFirstlyOpened = true;
                ChairmanName = "";
                TeachersCount = 1;
                TeacherNames = new string[] { "" };
                ExamintionDate = DateTime.Today.ToString("dd.MM.yyyy");
                InputDate = DateTime.Today.ToString("dd.MM.yyyy");
            }

            public static bool operator ==(SysSettingsFile a, SysSettingsFile b)
            {

                if ((object)a == null && (object)b == null)
                {
                    return true;
                }

                if ((object)a == null || (object)b == null)
                {
                    return false;
                }

                return a.IsFirstlyOpened.Equals(b.IsFirstlyOpened) && a.TeacherNames.Equals(b.TeacherNames);
            }
            public static bool operator !=(SysSettingsFile a, SysSettingsFile b) => !(a == b);

            public static void SetNewSettings(out SysSettingsFile settings, FileProperties newProps)
            {
                settings = new SysSettingsFile(false, newProps.HeadTeacherName, (byte)newProps.TeachersNames.Length, newProps.TeachersNames, newProps.ExaminationDate, newProps.InputDate);
                SaveFile((Application.Current as App).AppPath + (Application.Current as App).SettingsPath, settings);
                (Application.Current.MainWindow as MainWindow).actualFileProps = newProps;

                string path = (Application.Current.MainWindow as MainWindow).WorkWith_FilePath;
                if (path != null)
                {
                    (Application.Current.MainWindow as MainWindow).FsstFile.WriteToByteFSSTFile(path);
                }
                
            }
        }

        public static SysSettingsFile LoadSysSettingFileFromBytes(string path)
        {
            SysSettingsFile file = new SysSettingsFile();
            byte[] bytes;

            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Невозможно запустить приложение: файл конфигураций не был найден.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }


            MemoryStream memoryStream = new MemoryStream(bytes);
            BinaryReader br = new BinaryReader(memoryStream);

            try
            {
                
                file.IsFirstlyOpened = br.ReadBoolean();
                file.ChairmanName = br.ReadString();
                file.TeachersCount = br.ReadByte();
                file.TeacherNames = new string[file.TeachersCount];
                for (int i = 0; i < file.TeachersCount; i++)
                {
                    file.TeacherNames[i] = br.ReadString();
                }

            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Невозможно запустить приложение: файл конфигураций не действителен.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }

            

            return file;
        }

        public static void SaveFile(string path, SysSettingsFile file)
        {
            System.IO.File.WriteAllBytes(path, file.ToBytes());
        }

    }

    public static class SysSettingsFileExtensions
    {
        public static byte[] ToBytes(this Проекты_8_9_Классы.res.SystemSettings.SysSettingsFile file)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memoryStream);

            bw.Write(file.IsFirstlyOpened);
            bw.Write(file.ChairmanName);
            bw.Write(file.TeachersCount);
            for (int i = 0; i < file.TeachersCount; i++)
            {
                bw.Write(file.TeacherNames[i]);
            }

            return memoryStream.ToArray();

        }
    }

}
