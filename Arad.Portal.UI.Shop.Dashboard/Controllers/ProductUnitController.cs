using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductUnitController : Controller
    {
        // GET: ProductUnitController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ProductUnitController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductUnitController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductUnitController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductUnitController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductUnitController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductUnitController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductUnitController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
