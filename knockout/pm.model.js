var pm = pm || {};
pm.app = pm.app || {};

(function (app) {

	// ����� "����� �����" (��������� ����� ������ �����������)
	pm.app.CoinsArray = function (value, count) {
		var self = this;
		
		// �������
		self.value = ko.observable(value);
		// ����������
		self.count = ko.observable(count);
		
		// ����� ��������� ������ �����
		self.sum = ko.computed(function () {
			var value = self.value();
			var count = self.count();
			return value * count;
		});
		// ��������� ��������
		self.caption = ko.computed(function () {
			var value = self.value();
			var count = self.count();
			return value + ' ��� - ' + count + ' ��';
		});
		
		return self;
	};


	// ����� "����� �������"
	pm.app.ProductsArray = function (name, price, count) {
		var self = this;
		
		// ��������
		self.name = ko.observable(name);
		// ����
		self.price = ko.observable(price);
		// ����������
		self.count = ko.observable(count);

		// ��������� ��������
		self.caption = ko.computed(function () {
			var name = self.name();
			var price = self.price();
			var count = self.count();
			return name + ' - ' + price + ' ��� - ' + count + ' ��';
		});
		
		return self;
	};


	// ����� ������ ��������� ����������� ������
	pm.app.PmModel = function () {
		var self = this;
		
		// ������� ������������ (����� � ��������� �������)
		self.userPurse = ko.observableArray([
			new pm.app.CoinsArray(1, 10),
			new pm.app.CoinsArray(2, 30),
			new pm.app.CoinsArray(5, 20),
			new pm.app.CoinsArray(10, 15)
		]);
		// ������� ������ (����� � ��������� �������)
		self.vmPurse = ko.observableArray([
			new pm.app.CoinsArray(1, 100),
			new pm.app.CoinsArray(2, 100),
			new pm.app.CoinsArray(5, 100),
			new pm.app.CoinsArray(10, 100)
		]);
		// ��������� ����� (������ ����� ��� ������� �� ������, �.�. ��������� ��� ������ ��������� � �������� ������)
		self.deposit = ko.observable(0);
		
		// ����� ������� � ������ (����� � ��������� �������)
		self.vmProducts = ko.observableArray([
			new pm.app.ProductsArray('���', 13, 10),
			new pm.app.ProductsArray('����', 18, 20),
			new pm.app.ProductsArray('���� � �������', 21, 20),
			new pm.app.ProductsArray('���', 35, 15)
		]);
		// �������� ��������
		self.issuedProducts = ko.observableArray();
		
		// �������� ������ � ������
		self.replenishDeposit = function (userCoinsArray) {
			if (!userCoinsArray) { return; }
			if (!userCoinsArray.count) {
				app.logger.log('� �������� ������������ ��� ����� ����������� ' + userCoinsArray.value);
				return;
			}
			
			// ������� ������ �� �������� ������������
			userCoinsArray.count(userCoinsArray.count() - 1);
			// ��������� ������ � ������� ������
			var vmCoinsArray = getCoinsArray(self.vmPurse(), userCoinsArray.value());
			vmCoinsArray.count(vmCoinsArray.count() + 1);
			// ��������� ����� �� �������
			self.deposit(self.deposit() + userCoinsArray.value());

			app.logger.log(null);
		};
		// �������� �����
		self.getChange = function() {
			// ��� ����������� ������������ ���������� ����� ��� ����� ���������� "������ ��������".
			// �.�. � ��� ������ ������������ 10,5,2,1 - �� ��������.
			// ���������� ��������� - �� ������ ����� �������� ��� ���������� ��������� ����� � ������.
			// ������: 8 ������ ����� ������ 5+2+1 ��� 2+2+2+2. ������ �������� ������ ������ �������.
			// �� ���� � ������ ��� �������� ����� - ������ �������� �� ������ �������.
			
			var deposit = self.deposit();
			var vmPurse = self.vmPurse()
				.filter(function (el) { return el.count() > 0; })
				.sort(function(el1, el2) { return el1.value() < el2.value(); });
			var change = [];
			// ������ ��������
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
			// ����������� ��������� ������ ���������
			if (deposit > 0) {
				// ���� ����� �������� �� ��� ����� ������������ �� ������� - ������ �������� �� ���������
				app.logger.log('���������� ������ �����');
			} else {
				// ���� ��� ����� ������������ �� ������� - ��������� ��� ������ �� �������� ������ � ������� ������������
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
		// ������ �������
		self.issueProduct = function (product) {
			if (!product) { return; }
			if (!product.count()) { 
				app.logger.log('����� "' + product.name() + '" ����������');
				return;
			}
			var deposit = self.deposit();
			if (product.price() > deposit) { 
				app.logger.log('�� ����� "' + product.name() + '" �� ������� �����');
				return;
			}

			// ��������� ������ � ���������� ��������
			self.deposit(deposit - product.price());
			// ������ �����
			product.count(product.count() - 1);
			self.issuedProducts.push(product.name());

			app.logger.log(null);
		};
		
		// �������� ����� ����� ������� ����������� �� ��������
		function getCoinsArray(purse, coinValue) {
			var coinsArrays = purse.filter(function (el) { return el.value() == coinValue });
			// ���� � �������� ���� ����� ����� - ���������� ���
			if (coinsArrays.length) {
				return coinsArrays[0];
			}
			// ���� ��� - ������� ������ ����� ����� � ����������
			var coinsArray = new pm.app.CoinsArray(coinValue, 0);
			purse.push(coinsArray);
			return coinsArray;
		};
			
		return self;
	};

})(pm.app);
