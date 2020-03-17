using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vm.Areas.VendingMachine.Models.GetChangeAlgorithms
{
    /// <summary>
    /// Жадный алгоритм подбора сдачи
    /// </summary>
    public class GreedyGetChangeAlgorithm : IGetChangeAlgorithm
    {
        /// <summary>
        /// Подобрать сдачу минимальным количеством монет
        /// </summary>
        public List<CoinsArray> Process(VendingMachineModel model)
        {
            // Для определения минимального количества монет для сдачи используем "жадный алгоритм".
            // Т.к. у нас монеты достоинством 10,5,2,1 - он подходит.
            // Недостаток алгоритма - не всегда будет работать при недостатке некоторых монет в машине.
            // Пример: 8 рублей можно выдать 5+2+1 или 2+2+2+2. Жадный алгоритм выдаст первый вариант.
            // Но если в машине нет рублевых монет - жадный алгоритм не найдет решение.

            var deposit = model.Deposit;
            var vmPurse = model.VmPurse
                .Where(o => o.Count > 0)
                .OrderByDescending(o => o.Value)
                .ToList();
            var change = new List<CoinsArray>();
            // Жадный алгоритм
            foreach (var coinsArray in vmPurse)
            {
                var nominal = coinsArray.Value;
                var coinsCount = coinsArray.Count;
                if (nominal <= deposit)
                {
                    var changeCoinsCount = Convert.ToInt32(Math.Floor((deposit + 0.0) / nominal));
                    if (changeCoinsCount > coinsCount)
                    {
                        changeCoinsCount = coinsCount;
                    }

                    change.Add(new CoinsArray(nominal, changeCoinsCount));
                    deposit = deposit % nominal;
                }
            }

            // Анализируем результат работы алгоритма
            if (deposit > 0)
            {
                // Если после перебора не вся сумма распределена по монетам - значит алгоритм не отработал
                return null;
            }
            else
            {
                // Если вся сумма распределена по монетам - переносим эти монеты из кошелька машины в кошелек пользователя
                return change;
            }
        }
    }
}