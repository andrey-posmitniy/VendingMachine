var pm = pm || {};
pm.app = pm.app || {};

(function (app) {

	// ������
	pm.app.Logger = function (value) {
		var self = this;
		
		// ��������� ��� ������ �� ������ ����-������
		self.message = ko.observable();
		
		self.log = function (text) {
			console.log(text);
			self.message(text);
		};
		
		return self;
	};

})(pm.app);
