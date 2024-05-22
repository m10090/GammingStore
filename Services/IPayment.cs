namespace gammingStore.Services.Payment;

interface IPayment {
  string makePaymentLink();
  void isPaymentSuccess();
  void instertIntoTransactionTable();
}

