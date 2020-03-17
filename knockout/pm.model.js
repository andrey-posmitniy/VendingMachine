var pm = pm || {};
pm.app = pm.app || {};

(function (app) {

	// Класс "Набор монет" (несколько монет одного достоинства)
	pm.app.CoinsArray = function (value, count) {
		var self = this;
		
		// Номинал
		self.value = ko.observable(value);
		// Количество
		self.count = ko.observable(count);
		
		// Общая стоимость набора монет
		self.sum = ko.computed(function () {
			var value = self.value();
			var count = self.count();
			return value * count;
		});
		// Текстовое описание
		self.caption = ko.computed(function () {
			var value = self.value();
			var count = self.count();
			return value + ' руб - ' + count + ' шт';
		});
		
		return self;
	};


	// Класс "Набор товаров"
	pm.app.ProductsArray = function (name, price, count) {
		var self = this;
		
		// Название
		self.name = ko.observable(name);
		// Цена
		self.price = ko.observable(price);
		// Количество
		self.count = ko.observable(count);

		// Текстовое описание
		self.caption = ko.computed(function () {
			var name = self.name();
			var price = self.price();
			var count = self.count();
			return name + ' - ' + price + ' руб - ' + count + ' шт';
		});
		
		return self;
	};


	// Класс модели поведения вендинговой машины
	pm.app.PmModel = function () {
		var self = this;
		
		// Кошелек пользователя (сразу с исходными данными)
		self.userPurse = ko.observableArray([
			new pm.app.CoinsArray(1, 10),
			new pm.app.CoinsArray(2, 30),
			new pm.app.CoinsArray(5, 20),
			new pm.app.CoinsArray(10, 15)
		]);
		// Кошелек машины (сразу с исходными данными)
		self.vmPurse = ko.observableArray([
			new pm.app.CoinsArray(1, 100),
			new pm.app.CoinsArray(2, 100),
			new pm.app.CoinsArray(5, 100),
			new pm.app.CoinsArray(10, 100)
		]);
		// Внесенная сумма (просто сумма без деления на монеты, т.к. физически эти деньги находятся в кошельке машины)
		self.deposit = ko.observable(0);
		
		// Набор товаров в машине (сразу с исходными данными)
		self.vmProducts = ko.observableArray([
			new pm.app.ProductsArray('Чай', 13, 10),
			new pm.app.ProductsArray('Кофе', 18, 20),
			new pm.app.ProductsArray('Кофе с молоком', 21, 20),
			new pm.app.ProductsArray('Сок', 35, 15)
		]);
		// Выданные продукты
		self.issuedProducts = ko.observableArray();
		
		// Опустить монету в машину
		self.replenishDeposit = function (userCoinsArray) {
			if (!userCoinsArray) { return; }
			if (!userCoinsArray.count) {
				app.logger.log('В кошельке пользователя нет монет достоинства ' + userCoinsArray.value);
				return;
			}
			
			// Убираем монету из кошелька пользователя
			userCoinsArray.count(userCoinsArray.count() - 1);
			// Добавляем монету в кошелек машины
			var vmCoinsArray = getCoinsArray(self.vmPurse(), userCoinsArray.value());
			vmCoinsArray.count(vmCoinsArray.count() + 1);
			// Добавляем сумму на депозит
			self.deposit(self.deposit() + userCoinsArray.value());

			app.logger.log(null);
		};
		// Получить сдачу
		self.getChange = function() {
			// Для определения минимального количества монет для сдачи используем "жадный алгоритм".
			// Т.к. у нас монеты достоинством 10,5,2,1 - он подходит.
			// Недостаток алгоритма - не всегда будет работать при недостатке некоторых монет в машине.
			// Пример: 8 рублей можно выдать 5+2+1 или 2+2+2+2. Жадный алгоритм выдаст первый вариант.
			// Но если в машине нет рублевых монет - жадный алгоритм не найдет решение.
			
			var deposit = self.deposit();
			var vmPurse = self.vmPurse()
				.filter(function (el) { return el.count() > 0; })
				.sort(function(el1, el2) { return el1.value() < el2.value(); });
			var change = [];
			// Жадный алгоритм
			for (var i = 0; i < vmPurse.length; i++) {
				var nominal = vmPurse[i].value();
				var coinsCount = vmPurse[i].count();
                if (nominal <= deposit)
                {
					var changeCoinsCount = Math.floor(deposit / nominal);
					if (changeCoinsCount > coinsCount) {
						changeCoinsCount = coinsCount;
					}
					
                    change.push(new pm.app.CoinsArray(nominal, changeCoinsCount));
                    deposit = deposit % nominal;
                }
            }
			// Анализируем результат работы алгоритма
			if (deposit > 0) {
				// Если после перебора не вся сумма распределена по монетам - значит алгоритм не отработал
				app.logger.log('Невозможно выдать сдачу');
			} else {
				// Если вся сумма распределена по монетам - переносим эти монеты из кошелька машины в кошелек пользователя
				for (var i = 0; i < change.length; i++) {
					var changeCoinsArray = change[i];
					var userCoinsArray = getCoinsArray(self.userPurse(), changeCoinsArray.value());
					var vmCoinsArray = getCoinsArray(self.vmPurse(), changeCoinsArray.value());
					userCoinsArray.count(userCoinsArray.count() + changeCoinsArray.count());
					vmCoinsArray.count(vmCoinsArray.count() - changeCoinsArray.count());
					self.deposit(0);
				}
			
				app.logger.log(null);
			}
		};
		// Выдать продукт
		self.issueProduct = function (product) {
			if (!product) { return; }
			if (!product.count()) { 
				app.logger.log('Товар "' + product.name() + '" закончился');
				return;
			}
			var deposit = self.deposit();
			if (product.price() > deposit) { 
				app.logger.log('На товар "' + product.name() + '" не хватает денег');
				return;
			}

			// Списываем деньги с внесенного депозита
			self.deposit(deposit - product.price());
			// Выдаем товар
			product.count(product.count() - 1);
			self.issuedProducts.push(product.name());

			app.logger.log(null);
		};
		
		// Получить набор монет нужного достоинства из кошелька
		function getCoinsArray(purse, coinValue) {
			var coinsArrays = purse.filter(function (el) { return el.value() == coinValue });
			// Если в кошельке есть набор монет - возвращаем его
			if (coinsArrays.length) {
				return coinsArrays[0];
			}
			// Если нет - создаем пустой набор монет и возвращаем
			var coinsArray = new pm.app.CoinsArray(coinValue, 0);
			purse.push(coinsArray);
			return coinsArray;
		};
			
		return self;
	};

})(pm.app);
