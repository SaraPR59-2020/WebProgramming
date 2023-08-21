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

            return View(ispitiProfesora);
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
            var ispitiProfesora = _ispiti.ReadFromFile().Where(i => i.Profesor.Equals(imeProfesora)).ToList();
            var rezultatiIspita = _rezultati.ReadFromFile().Where(r => ispitiProfesora.Any(i => i.Id == r.Ispit.Id)).ToList();

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

            var rezultat = _rezultati.ReadFromFile().FirstOrDefault(r => r.Ispit.Id == id);

            if (rezultat != null)
            {
                rezultat.Ocena = ocena;
                var sviRezultati = _rezultati.ReadFromFile().Where(r => r.Ispit.Id != id).ToList();
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
    }
}