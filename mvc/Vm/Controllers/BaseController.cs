﻿using System.Web.Mvc;

namespace Vm.Controllers
{
    /// <summary>
    /// Базовый контроллер.
    /// Нужен для реализации базовых для всех контроллеров функций, например для логирования, предоставления общих данных (текущего юзера) и т.п.
    /// Лучше сразу отнаследоваться от базового класса, чем спустя несколько лет пробегать по всем контроллерам.
    /// </summary>
    public abstract class BaseController : Controller
    {
    }
}