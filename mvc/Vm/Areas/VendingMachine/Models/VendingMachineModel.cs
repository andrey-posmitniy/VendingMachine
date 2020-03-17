using System;
using System.Collections.Generic;
using System.Linq;
using Vm.Areas.VendingMachine.Models.GetChangeAlgorithms;
using Vm.Utils;

namespace Vm.Areas.VendingMachine.Models
{
    /// <summary>
    /// Модель поведения вендинговой машины
    /// </summary>
    public class VendingMachineModel
    {
		/// <summary>
		/// Кошелек пользователя
		/// </summary>
        public List<CoinsArray> UserPurse { get; private set; }
        /// <summary>
        /// Кошелек машины
        /// </summary>
        public List<CoinsArray> VmPurse { get; private set; }
		/// <summary>
        /// Внесенная сумма (просто сумма без деления на монеты, т.к. физически эти деньги находятся в кошельке машины)
		/// </summary>
        public int Deposit { get; private set; }
		
        /// <summary>
        /// Набор товаров в машине (сразу с исходными данными)
        /// </summary>
        public List<ProductsArray> VmProducts { get; private set; }
		/// <summary>
        /// Выданные продукты
		/// </summary>
        public List<string> IssuedProducts { get; private set; }

        /// <summary>
        /// Сообщение для отображения пользователю
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Объект для обеспечения потокобезопасности
        /// </summary>
        private readonly object locker = new object();


        public VendingMachineModel()
        {
            UserPurse = new List<CoinsArray>();
            VmPurse = new List<CoinsArray>();
            VmProducts = new List<ProductsArray>();
            IssuedProducts = new List<string>();

            // Исходные данные
            
            UserPurse.Add(new CoinsArray(1, 10));
            UserPurse.Add(new CoinsArray(2, 30));
            UserPurse.Add(new CoinsArray(5, 20));
            UserPurse.Add(new CoinsArray(10, 15));

            VmPurse.Add(new CoinsArray(1, 100));
            VmPurse.Add(new CoinsArray(2, 100));
            VmPurse.Add(new CoinsArray(5, 100));
            VmPurse.Add(new CoinsArray(10, 100));

            VmProducts.Add(new ProductsArray(1, "Чай", 13, 10));
            VmProducts.Add(new ProductsArray(2, "Кофе", 18, 20));
            VmProducts.Add(new ProductsArray(3, "Кофе с молоком", 21, 20));
            VmProducts.Add(new ProductsArray(4, "Сок", 35, 15));
        }


        /// <summary>
        /// Опустить монету в машину
        /// </summary>
        /// <param name="coinValue">Номинал монеты</param>
        public void ReplenishDeposit(int coinValue)
        {
            lock (locker)
            {
                var userCoinsArray = GetCoinsArray(UserPurse, coinValue);
                if (userCoinsArray.Count < 1)
                {
                    this.Message = string.Format("В кошельке пользователя нет монет достоинства {0}",
                        userCoinsArray.Value);
                    return;
                }

                // Убираем монету из кошелька пользователя
                userCoinsArray.Count--;
                // Добавляем монету в кошелек машины
                var vmCoinsArray = GetCoinsArray(VmPurse, coinValue);
                vmCoinsArray.Count++;
                // Добавляем сумму на депозит
                this.Deposit += userCoinsArray.Value;

                this.Message = null;
            }
        }

        /// <summary>
        /// Получить сдачу
        /// </summary>
        /// <returns>Список наборов монет, которыми выдается сдача. Или null, если невозможно выдать сдачу.</returns>
        public List<CoinsArray> GetChange()
        {
            lock (locker)
            {
                var getChangeAlgorithm = IoC.Container.GetInstance<IGetChangeAlgorithm>();
                var change = getChangeAlgorithm.Process(this);

                // Анализируем результат работы алгоритма
                if (change == null)
                {
                    // Если после перебора не вся сумма распределена по монетам - значит алгоритм не отработал
                    Message = "Невозможно выдать сдачу";
                    return null;
                }
                else
                {
                    // Если вся сумма распределена по монетам - переносим эти монеты из кошелька машины в кошелек пользователя
                    foreach (var changeCoinsArray in change)
                    {
                        var userCoinsArray = GetCoinsArray(UserPurse, changeCoinsArray.Value);
                        var vmCoinsArray = GetCoinsArray(VmPurse, changeCoinsArray.Value);
                        userCoinsArray.Count += changeCoinsArray.Count;
                        vmCoinsArray.Count -= changeCoinsArray.Count;
                        this.Deposit = 0;
                    }

                    this.Message = null;
                    return change;
                }
            }
        }

        /// <summary>
        /// Выдать продукт
        /// </summary>
        public void IssueProduct(int productId)
        {
            lock (locker)
            {
                var product = VmProducts.FirstOrDefault(o => o.Id == productId);
                if (product == null)
                {
                    this.Message = "Товар не найден";
                    return;
                }
                if (product.Count < 0)
                {
                    this.Message = string.Format("Товар \"{0}\" закончился", product.Name);
                    return;
                }

                if (product.Price > Deposit)
                {
                    this.Message = string.Format("На товар \"{0}\" не хватает денег", product.Name);
                    return;
                }

                // Списываем деньги с внесенного депозита
                Deposit -= product.Price;
                // Выдаем товар
                product.Count--;
                this.IssuedProducts.Add(product.Name);

                this.Message = null;
            }
        }


        /// <summary>
        /// Получить набор монет нужного достоинства из кошелька
        /// </summary>
        /// <param name="purse">Кошелек</param>
        /// <param name="coinValue">Номинал монеты</param>
        private CoinsArray GetCoinsArray(List<CoinsArray> purse, int coinValue)
        {
            var coinsArray = purse.FirstOrDefault(o => o.Value == coinValue);

            // Если в кошельке есть набор монет - возвращаем его
            if (coinsArray != null)
            {
                return coinsArray;
            }

            // Если нет - создаем пустой набор монет и возвращаем
            coinsArray = new CoinsArray(coinValue, 0);
            purse.Add(coinsArray);
            return coinsArray;
        }
    }
}