using HelloWorld.Teste;

namespace HelloWorld;   

public class Program
{
    static void Main(string[] args)
    {
        //void ChamandoConsoleWriteLine(string text)
        //{
        //    Console.WriteLine(text);
        //}

        //ChamandoConsoleWriteLine("Oi, eu sou o goku!");
        //ChamandoConsoleWriteLine("Hello World!");

        Carro meuCarro = new Carro();

        meuCarro.Ligar();
        meuCarro.Desligar();
        meuCarro.Teste2();

        Biscoito meuBiscoito = new Biscoito();

        meuBiscoito.Temperatura();
    }
}