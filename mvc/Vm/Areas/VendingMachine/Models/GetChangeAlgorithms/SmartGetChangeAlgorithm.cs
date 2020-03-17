using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vm.Areas.VendingMachine.Models.GetChangeAlgorithms
{
    /// <summary>
    /// Умный алгоритм подбора сдачи
    /// </summary>
    public class SmartGetChangeAlgorithm : IGetChangeAlgorithm
    {
        /*
        const
          Coins: array[0..8] of Integer = (1, 2, 3, 5, 10, 15, 20, 50, 1000000);
        //большое число - терминатор, без него нужно будет проверять выход индекса за границу массива в условии while
 
        procedure Razmen(Sum, StartingCoinIndex: Integer; s: string);
        begin
          if Sum = 0 then
            Вывести s
          else
            while Coins[StartingCoinIndex] <= Sum do begin
              Razmen(Sum - Coins[StartingCoinIndex], StartingCoinIndex, s + IntToStr(Coins[StartingCoinIndex]) + ' ');
              Inc(StartingCoinIndex);
            end;
        end;
 
        Вызов
         Razmen(11, 0, '');
        */

        /// <summary>
        /// Набор доступныъ номиналов монет
        /// </summary>
        private int[] coins;
        //private readonly int[] coins = new int[] { 1, 2, 5, 10 };


        /// <summary>
        /// Подобрать сдачу минимальным количеством монет
        /// </summary>
        public List<CoinsArray> Process(VendingMachineModel model)
        {
            this.coins = model.VmPurse
                .Where(o => o.Count > 0)
                .Select(o => o.Value)
                .OrderBy(o => o)
                .ToArray();

            // Получаем все возможные наборы монет для формирования нужной суммы
            var results = new List<ChangeCoinsList>();
            Process(model.Deposit, 0, null, results);
            // Сортируем по возрастанию количества монет, а затем по убыванию самой большой монеты
            results = results
                .OrderBy(o => o.Coins.Count)
                .ThenBy(o => o.Coins.Last())
                .ToList();

            // Отсекаем те наборы монет, которые не могут быть получены из текущего кошелька машины
            var firstCoinsArrays = results
                .Select(o =>
                {
                    var coinsArrays = o.Coins
                        .GroupBy(c => c)
                        .Select(c => new CoinsArray(c.Key, c.Count()))
                        .ToList();
                    return coinsArrays;
                })
                .Where(o =>
                {
                    foreach (var coinsArray in o)
                    {
                        //if (!model.VmPurse.Any(x => x.Value == coinsArray.Value && x.Count < coinsArray.Count))
                        if (!model.VmPurse.Any(x => x.Value == coinsArray.Value && x.Count >= coinsArray.Count))
                        {
                            // Если монет какого-то номинала не хватает в кошельке машины - значит данный набор монет не подходит
                            return false;
                        }
                    }
                    return true;
                })
                .FirstOrDefault();

            // Анализируем результат работы алгоритма
            // Если firstCoinsArrays == null - значит алгоритм не отработал
            // Иначе - если вся сумма распределена по монетам - переносим эти монеты из кошелька машины в кошелек пользователя
            return firstCoinsArrays;
        }



        private void Process(int sum, int startingCoinIndex, ChangeCoinsList coinsList, List<ChangeCoinsList> results)
        {
            if (coinsList == null)
            {
                coinsList = new ChangeCoinsList();
            }

            if (sum == 0)
            {
                // Текущий набор монет полностью охватил нужную сумму - фиксируем результат
                results.Add(coinsList);
            }
            else
            {
                while (startingCoinIndex < coins.Length && coins[startingCoinIndex] <= sum)
                {
                    var newCoinsList = coinsList.Clone();
                    newCoinsList.Coins.Add(coins[startingCoinIndex]);

                    Process(
                        sum - coins[startingCoinIndex],
                        startingCoinIndex,
                        newCoinsList,
                        results);
                    startingCoinIndex++;
                }
            }
        }

        //public void ProcessOriginal(int sum, int startingCoinIndex, string s)
        //{
        //    if (sum == 0)
        //    {
        //        Console.WriteLine(s);
        //    }
        //    else
        //    {
        //        while (coins[startingCoinIndex] <= sum)
        //        {
        //            Process
        //                (sum - coins[startingCoinIndex],
        //                    startingCoinIndex,
        //                    s + coins[startingCoinIndex].ToString() + ' ')
        //                ;
        //            startingCoinIndex++;
        //        }
        //    }
        //}


        /// <summary>
        /// Набор монет
        /// </summary>
        public class ChangeCoinsList
        {
            public List<int> Coins { get; set; }

            public ChangeCoinsList()
            {
                Coins = new List<int>();
            }

            public ChangeCoinsList Clone()
            {
                return new ChangeCoinsList
                {
                    Coins = Coins.ToList()
                };
            }

            public override string ToString()
            {
                return string.Join("+", Coins);
            }
        }
    }
}