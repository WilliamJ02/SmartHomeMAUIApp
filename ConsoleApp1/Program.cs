using Shared.Azure.Communications;

var email = new EmailCommunication();

email.Send("william.jarnebrant@hotmail.com", "hej", "Hej", "hello world");