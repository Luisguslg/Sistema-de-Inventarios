using Microsoft.Extensions.Hosting;

namespace IITS.Services
{
    public class LogsGenerator
    {
        private readonly IWebHostEnvironment _hostenvironment;

        public LogsGenerator(IWebHostEnvironment HostEnvironment)
        {
            _hostenvironment = HostEnvironment;
        }

        public void ErrorLog(string errordetail, string errortitle)
        {
            string first = Path.Combine(_hostenvironment.ContentRootPath, "Logs");
            string second = errortitle;

            var path = Path.Combine(first, second);

            if (File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    string texto = "Archivo Log creado: " + DateTime.Now.ToString("dddd, dd MMMM yyyy");
                    sw.WriteLine(texto);
                }
            }
            RegistrarLogTexto(errordetail, path);
        }

        public void RegistrarLogTexto(string log, string path)
        {
            if (!String.IsNullOrEmpty(log))
            {
                string fecha = DateTime.Now.ToString();

                string texto = fecha + ": " + log;

                File.AppendAllText(path, texto + Environment.NewLine);
            }
        }
    }
}
