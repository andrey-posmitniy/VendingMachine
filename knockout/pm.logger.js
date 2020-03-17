var pm = pm || {};
pm.app = pm.app || {};

(function (app) {

	// Логгер
	pm.app.Logger = function (value) {
		var self = this;
		
		// Сообщение для вывода на экране кофе-машины
		self.message = ko.observable();
		
		self.log = function (text) {
			console.log(text);
			self.message(text);
		};
		
		return self;
	};

})(pm.app);
