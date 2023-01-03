
namespace Plugins.Shared.Library.Librarys
{

    public class Password
    {
        public string value;
        public Password(string content)
        {
            value = content;
        }


        public static implicit operator string(Password password)
        {
            return password.value;
        }

        public static implicit operator Password(string content)
        {
            return new Password(content);
        }
    }


    public class InputFile
    {
        public string value;
        public InputFile(string path)
        {
            value = path;
        }

        public static implicit operator string(InputFile file)
        {
            return file.value;
        }

        public static implicit operator InputFile(string path)
        {
            return new InputFile(path);
        }
    }
}
