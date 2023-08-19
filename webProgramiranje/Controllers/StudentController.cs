using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webProgramiranje.DB;
using webProgramiranje.Models;

namespace webProgramiranje.Controllers
{
    public class StudentController : Controller
    {
        private readonly JsonFileService<Student> _studenti; // pretpostavljajući da imate osnovni User model
        private readonly JsonFileService<Ispit> _ispiti; // pretpostavljajući da imate osnovni User model
        private readonly JsonFileService<RezultatIspita> _rezultatiIspita; // pretpostavljajući da imate osnovni User model


        public StudentController()
        {
            _studenti = new JsonFileService<Student>("studenti.json");
            _ispiti = new JsonFileService<Ispit>("ispiti.json");
            _rezultatiIspita = new JsonFileService<RezultatIspita>("rezultatiIspita.json");
        }

        // GET: Student
        public ActionResult Index()
        {
            var student = _studenti.ReadFromFile().FirstOrDefault(p => p.KorisnickoIme.Equals(HttpContext.Session["Username"]));
            var mojiIspiti = student.ListaIspita;
            return View(mojiIspiti);
        }
    }
}