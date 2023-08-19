using System.Linq;
using System.Web.Mvc;
using webProgramiranje.DB;
using webProgramiranje.Models;

namespace webProgramiranje.Controllers
{
    public class HomeController : Controller
    {
        private readonly JsonFileService<Student> _studenti; // pretpostavljajući da imate osnovni User model
        private readonly JsonFileService<Profesor> _profesori; // pretpostavljajući da imate osnovni User model
        private readonly JsonFileService<Administrator> _administratori; // pretpostavljajući da imate osnovni User model

        public HomeController()
        {
            _studenti = new JsonFileService<Student>("studenti.json");
            _profesori = new JsonFileService<Profesor>("profesori.json");
            _administratori = new JsonFileService<Administrator>("administratori.json");
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login()
        {
            var username = Request["username"];
            var password = Request["password"];
            var studenti = _studenti.ReadFromFile();
            var profesori = _profesori.ReadFromFile();
            var administratori = _administratori.ReadFromFile();
            var student = studenti!= null ? studenti.FirstOrDefault(u => u.KorisnickoIme == username && u.Sifra == password): null ;
            var profesor = profesori != null ? profesori.FirstOrDefault(u => u.KorisnickoIme == username && u.Sifra == password) : null;
            var administrator = administratori != null ? administratori.FirstOrDefault(u => u.KorisnickoIme == username && u.Sifra == password) : null;

            if (student != null || profesor != null || administrator != null)
            {
                // Postavljanje vrednosti u sesiji:
                HttpContext.Session["Username"] = username;

                if (student != null)
                {
                    HttpContext.Session["UserRole"] = "Student";
                    return RedirectToAction("Index", "Student");
                }
                else if (profesor != null)
                {
                    HttpContext.Session["UserRole"] = "Profesor";
                    return RedirectToAction("Index", "Profesor");
                }
                else if (administrator != null) {
                    HttpContext.Session["UserRole"] = "Administrator";
                    return RedirectToAction("Index", "Admin");
                }
                
            }
            else
            {
                TempData["Error"] = "Neispravno korisničko ime ili lozinka.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

    }
}