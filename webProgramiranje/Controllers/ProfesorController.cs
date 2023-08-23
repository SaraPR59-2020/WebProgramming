using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webProgramiranje.DB;
using webProgramiranje.Models;

namespace webProgramiranje.Controllers
{
    public class ProfesorController : Controller
    {
        private readonly JsonFileService<Profesor> _profesori;
        private readonly JsonFileService<RezultatIspita> _rezultati;
        private readonly JsonFileService<Ispit> _ispiti;
        private readonly JsonFileService<Student> _studenti;


        public ProfesorController()
        {
            _profesori = new JsonFileService<Profesor>("profesori.json");
            _ispiti = new JsonFileService<Ispit>("ispiti.json");
            _rezultati = new JsonFileService<RezultatIspita>("rezultatiIspita.json");
            _studenti = new JsonFileService<Student>("studenti.json");
        }

        // GET: Profesor
        public ActionResult Index()
        {
            // Pretpostavljamo da je ime profesora sačuvano u sesiji kao "Username"
            var imeProfesora = HttpContext.Session["Username"].ToString();

            var ispiti = _ispiti.ReadFromFile();
            // Filtrirajte ispite na osnovu imena profesora
            var ispitiProfesora =ispiti != null ? ispiti.Where(i => i.Profesor.Equals(imeProfesora)).ToList():new List<Ispit>();
            var rez = _rezultati.ReadFromFile();
            var rezProfesora = rez != null ? rez.Where(r => ispitiProfesora.Any(ip => ip.Id == r.Ispit.Id)).ToList() : new List<RezultatIspita>();
            return View(rezProfesora);
        }

        // GET: Profesor/CreateIspit
        public ActionResult CreateIspit()
        {
            var trenutniProfesor = _profesori.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));

            // Pretpostavljajući da trenutniProfesor.ListaPredmeta sadrži listu stringova sa nazivima predmeta
            ViewBag.Predmeti = trenutniProfesor.ListaPredmeta.Select(p => new SelectListItem { Value = p, Text = p }).ToList();

            return View(new Ispit());
        }

        [HttpPost]
        public ActionResult CreateIspit(Ispit ispit)
        {
            var trenutniProfesor = _profesori.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            ViewBag.Predmeti = trenutniProfesor.ListaPredmeta.Select(p => new SelectListItem { Value = p, Text = p }).ToList();


            if (ispit.DatumIVremeOdrzavanja < DateTime.Now)
            {
                ModelState.AddModelError("DatumIVremeOdrzavanja", "Ne možete odabrati prošli datum i vreme.");
            }

            if (!trenutniProfesor.ListaPredmeta.Contains(ispit.Predmet))
            {
                ModelState.AddModelError("Predmet", "Možete zakažati ispit samo za svoj predmet.");
            }

            if (ModelState.IsValid)
            {
                var ispiti = _ispiti.ReadFromFile() !=null? _ispiti.ReadFromFile().ToList(): new List<Ispit>();
                ispit.Id = ispiti.Count;
                ispiti.Add(ispit);
                _ispiti.WriteToFile(ispiti);
                return RedirectToAction("Index"); // Preusmerite na glavnu stranicu ili gde god želite nakon uspešnog kreiranja ispita.
            }

            return View(ispit); // Vratite se nazad na formu sa greškama.
        }


        // GET: Profesor/OcenjivanjeIspita
        public ActionResult OcenjivanjeIspita()
        {
            var imeProfesora = HttpContext.Session["Username"].ToString();

            // Dohvatanje svih rezultata ispita koje je profesor zakazao
            var ispitiProfesora = _ispiti.ReadFromFile()!=null? _ispiti.ReadFromFile().Where(i => i.Profesor.Equals(imeProfesora)).ToList(): new List<Ispit>();
            var rezultatiIspita = _rezultati.ReadFromFile() != null? _rezultati.ReadFromFile().Where(r => ispitiProfesora.Any(i => i.Id == r.Ispit.Id)).ToList(): new List<RezultatIspita>();

            return View(rezultatiIspita);
        }

        [HttpPost]
        public ActionResult OcenjivanjeIspita(int id, int ocena)
        {
            if (ocena < 5 || ocena > 10)
            {
                TempData["ErrorMessage"] = "Ocena mora biti između 5 i 10!";
                return RedirectToAction("OcenjivanjeIspita");
            }

            var rezultat = _rezultati.ReadFromFile().FirstOrDefault(r => r.Id == id);

            if (rezultat != null)
            {
                rezultat.Ocena = ocena;
                var sviRezultati = _rezultati.ReadFromFile().Where(r => r.Id != id).ToList();
                sviRezultati.Add(rezultat);
                _rezultati.WriteToFile(sviRezultati);
               TempData["SuccessMessage"] = "Uspešno ste ocenili ispit!";
            }
            else
            {
                TempData["ErrorMessage"] = "Greška prilikom ocenjivanja ispita!";
            }

            return RedirectToAction("OcenjivanjeIspita");
        }

        [HttpPost]
        public ActionResult FilterAndSortExams(string rokFilter, string predmetFilter, int? ocenaFilter, string imeFilter, string prezimeFilter, string indeksFilter, string sortCriteria, string sortOrder)
        {
            // Ova linija je pretpostavka. Morate je zameniti sa stvarnim kodom koji će dohvatiti sve ispite za trenutnog profesora.
            var sviIspiti = _ispiti.ReadFromFile();
            // Filtrirajte ispite na osnovu imena profesora
            var ispitiProfesora = sviIspiti != null ? sviIspiti.Where(i => i.Profesor.Equals(HttpContext.Session["Username"].ToString())).ToList() : new List<Ispit>();
            var rez = _rezultati.ReadFromFile();
            var ispiti = rez != null ? rez.Where(r => ispitiProfesora.Any(ip => ip.Id == r.Ispit.Id)).ToList() : new List<RezultatIspita>();

            // Filtriranje
            if (!string.IsNullOrEmpty(rokFilter))
            {
                ispiti = ispiti.Where(i => i.Ispit.NazivIspitnogRoka.ToLower().Contains(rokFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(predmetFilter))
            {
                ispiti = ispiti.Where(i => i.Ispit.Predmet.ToLower().Contains(predmetFilter)).ToList();
            }
            if (ocenaFilter.HasValue)
            {
                ispiti = ispiti.Where(i => i.Ocena == ocenaFilter.Value).ToList();
            }
            if (!string.IsNullOrEmpty(imeFilter))
            {
                // Ova linija je pretpostavka. Ako imate različitu strukturu modela, prilagodite je.
                ispiti = ispiti.Where(i => i.Student.Ime.ToLower().Contains(imeFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(prezimeFilter))
            {
                // Slično kao za ime
                ispiti = ispiti.Where(i => i.Student.Prezime.ToLower().Contains(prezimeFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(indeksFilter))
            {
                // Slično kao za ime
                ispiti = ispiti.Where(i => i.Student.BrojIndeksa.ToLower().Contains(indeksFilter)).ToList();
            }

            // Sortiranje
            Func<RezultatIspita, object> orderFunc = i => i.Ispit.NazivIspitnogRoka; // Defaultno sortiranje po ispitnom roku

            switch (sortCriteria)
            {
                case "Rok":
                    orderFunc = i => i.Ispit.NazivIspitnogRoka;
                    break;
                case "Predmet":
                    orderFunc = i => i.Ispit.Predmet;
                    break;
                case "Ocena":
                    orderFunc = i => i.Ocena;
                    break;
                case "Ime":
                    orderFunc = i => i.Student.Ime; // Pretpostavka je da ispit ima referencu na studenta
                    break;
                case "Prezime":
                    orderFunc = i => i.Student.Prezime; // Slično kao za ime
                    break;
                case "Indeks":
                    orderFunc = i => i.Student.BrojIndeksa; // Slično kao za ime i prezime
                    break;
            }

            if (sortOrder == "Asc")
            {
                ispiti = ispiti.OrderBy(orderFunc).ToList();
            }
            else
            {
                ispiti = ispiti.OrderByDescending(orderFunc).ToList();
            }

            return View("Index", ispiti);
        }

    }
}