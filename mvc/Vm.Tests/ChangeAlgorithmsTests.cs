using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using Vm.Areas.VendingMachine.Models;
using Vm.Areas.VendingMachine.Models.GetChangeAlgorithms;
using Vm.Utils;

namespace Vm.Tests
{
    /// <summary>
    /// Тесты алгоритмов подбора сдачи
    /// </summary>
    [TestClass]
    public class ChangeAlgorithmsTests
    {
        /// <summary>
        /// Тест жадного алгоритма подбора сдачи
        /// </summary>
        [TestMethod]
        public void GreedyGetChangeTest()
        {
            ConfiguireIoc<GreedyGetChangeAlgorithm>();

            // Для определения минимального количества монет для сдачи используем "жадный алгоритм".
            // Т.к. у нас монеты достоинством 10,5,2,1 - он подходит.
            // Недостаток алгоритма - не всегда будет работать при недостатке некоторых монет в машине.
            // Пример: 8 рублей можно выдать 5+2+1 или 2+2+2+2. Жадный алгоритм выдаст первый вариант.
            // Но если в машине нет рублевых монет - жадный алгоритм не найдет решение.

            // Создаем модель с расчетом на то, что исходное состояние уже содержит нужное количество монет разных номиналов (исходные данные из БТ).
            // Если такой уверенности нет - значит нужно было бы здесь заполнить модель исходными значениями.
            var model = new VendingMachineModel();

            // Опускаем в машину 8 рублей (неважно какими монетами)
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);

            // Запрашиваем сдачу
            var change = model.GetChange();
            // Проверяем - какими монетами выдана сдача
            var changeUserCoins = change.ToDictionary(o => o.Value, o => o.Count);
            Assert.AreEqual(1, changeUserCoins.ContainsKey(5) ? changeUserCoins[5] : (int?)null);
            Assert.AreEqual(1, changeUserCoins.ContainsKey(2) ? changeUserCoins[2] : (int?)null);
            Assert.AreEqual(1, changeUserCoins.ContainsKey(1) ? changeUserCoins[1] : (int?)null);

            // Снова опускаем в машину 8 рублей (неважно какими монетами)
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            // Эмулируем ситуацию, когда в кошельке машины нет монет по 1р
            model.VmPurse
                .Where(o => o.Value == 1)
                .ToList()
                .ForEach(o =>
                {
                    o.Count = 0;
                });
            // Запрашиваем сдачу - жадный алгоритм выдаст ошибку
            change = model.GetChange();
            Assert.IsNull(change);
            Assert.IsTrue(!string.IsNullOrEmpty(model.Message));
        }

        /// <summary>
        /// Тест умного алгоритма подбора сдачи
        /// </summary>
        [TestMethod]
        public void SmartGetChangeTest()
        {
            ConfiguireIoc<SmartGetChangeAlgorithm>();

            // Создаем модель с расчетом на то, что исходное состояние уже содержит нужное количество монет разных номиналов (исходные данные из БТ).
            // Если такой уверенности нет - значит нужно было бы здесь заполнить модель исходными значениями.
            var model = new VendingMachineModel();

            // Опускаем в машину 8 рублей (неважно какими монетами)
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);

            // Запрашиваем сдачу
            var change = model.GetChange();
            // Проверяем - какими монетами выдана сдача
            var changeUserCoins = change.ToDictionary(o => o.Value, o => o.Count);
            Assert.AreEqual(1, changeUserCoins.ContainsKey(5) ? changeUserCoins[5] : (int?)null);
            Assert.AreEqual(1, changeUserCoins.ContainsKey(2) ? changeUserCoins[2] : (int?)null);
            Assert.AreEqual(1, changeUserCoins.ContainsKey(1) ? changeUserCoins[1] : (int?)null);

            // Снова опускаем в машину 8 рублей (неважно какими монетами)
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            model.ReplenishDeposit(2);
            // Эмулируем ситуацию, когда в кошельке машины нет монет по 1р
            model.VmPurse
                .Where(o => o.Value == 1)
                .ToList()
                .ForEach(o =>
                {
                    o.Count = 0;
                });
            // Запрашиваем сдачу
            change = model.GetChange();
            // Проверяем - какими монетами выдана сдача
            changeUserCoins = change.ToDictionary(o => o.Value, o => o.Count);
            Assert.AreEqual(4, changeUserCoins.ContainsKey(2) ? changeUserCoins[2] : (int?)null);

            // Снова опускаем в машину 2 рубля (неважно какими монетами)
            model.ReplenishDeposit(2);
            // Эмулируем ситуацию, когда в кошельке машины нет монет по 1р и 2р
            model.VmPurse
                .Where(o => o.Value < 3)
                .ToList()
                .ForEach(o =>
                {
                    o.Count = 0;
                });
            // Запрашиваем сдачу - умный алгоритм выдаст ошибку
            change = model.GetChange();
            Assert.IsNull(change);
            Assert.IsTrue(!string.IsNullOrEmpty(model.Message));
        }

        /// <summary>
        ///  Инициализация IOC-контейнера с заданием конкретного алгоритма подбора сдачи
        /// </summary>
        /// <typeparam name="TGetChangeAlgorithm"></typeparam>
        private void ConfiguireIoc<TGetChangeAlgorithm>()
            where TGetChangeAlgorithm : class, IGetChangeAlgorithm
        {
            var container = new Container();

            // Алгоритм подбора сдачи
            container.Register<IGetChangeAlgorithm, TGetChangeAlgorithm>();

            container.Verify();
            IoC.Container = container;
        }
    }
}
