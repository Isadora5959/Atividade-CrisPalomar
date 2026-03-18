using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Biblioteca
{
    public class usuario{
        public string EmailUsuario { get; set; }
        public string NomeUsuario { get; set; }
        public double MultiplicadorMulta{get;set;}
    }

    public class Livro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Genero { get; set; }
        public bool Disponivel { get; set; }
        public DateTime? DataEmprestimo { get; set; }

    }

    public class Atraso{
        
        public Atraso(Livro livro, ServicoEmail email,Usuario user){
            _livro=livro
            _email=email
            _user=user

        }

        public decimal CalcularMulta()
        {
            if (_livro.DataEmprestimo == null) return 0;
            var diasAtraso = (DateTime.Now - _livro.DataEmprestimo.Value).Days - 14;
            if (diasAtraso <= 0) return 0;
            return diasAtraso * 2.50m;
        }

        // Responsabilidade de comunicação: enviar e-mail
        public void EnviarEmailAtraso()
        {
            var multa = CalcularMulta();
            if (multa > 0)
            {
                _email.Enviar(EmailUsuario,Assunto,conteudo);
                var conteudo = $"Olá {_user.NomeUsuario}, você tem uma multa de R${multa} pelo livro '{_livro.Titulo}'.";
                Console.WriteLine($"[EMAIL] Para: {_user.EmailUsuario} | Mensagem: {conteudo}");
               
            }
        }
    }

    public class ServicoEmprestimo
    {
        public decimal CalcularDesconto(Usuario user, decimal valorMulta)
        {
            return ValorMulta * user.getMultiplicadorMulta();
        }

        public void RealizarEmprestimo(Livro livro, string nomeUsuario, string emailUsuario)
        {
            if (!livro.Disponivel)
            {
                Console.WriteLine("Livro indisponível.");
                return;
            }

            livro.Disponivel = false;
            livro.DataEmprestimo = DateTime.Now;
            livro.NomeUsuario = nomeUsuario;
            livro.EmailUsuario = emailUsuario;
            livro.SalvarNoBanco();

            Console.WriteLine($"Empréstimo realizado: {livro.Titulo} para {nomeUsuario}");
        }

        public void DevolverLivro(Livro livro, string tipoUsuario)
        {
            var multa = livro.CalcularMulta();
            var desconto = CalcularDesconto(tipoUsuario, multa);
            var multaFinal = multa - desconto;

            livro.Disponivel = true;
            livro.DataEmprestimo = null;
            livro.SalvarNoBanco();

            if (multaFinal > 0)
            {
                Console.WriteLine($"Devolução com multa de R${multaFinal}");
                livro.EnviarEmailAtraso();
            }
            else
            {
                Console.WriteLine("Devolução sem multa. Obrigado!");
            }
        }
    }

    public interface IEmprestar{
        public void Emprestar();
        public void Devolver();
    }

    public interface IFisico{
        public void ReservarItem();
    }

    public class LivroFisico : Livro,IEmprestar,IFisico
    {
        public override void Emprestar(string usuario)
        {
            Disponivel = false;
            Console.WriteLine($"[FÍSICO] '{Titulo}' emprestado para {usuario}.");
        }

        public override void Devolver()
        {
            Disponivel = true;
            Console.WriteLine($"[FÍSICO] '{Titulo}' devolvido.");
        }

        public override void ReservarItem(string usuario)
        {
            Console.WriteLine($"[FÍSICO] '{Titulo}' reservado para {usuario} por 3 dias.");
        }
    }

    public class EbookEmprestavel : Livro,IEmprestar
    {
        public override void Emprestar(string usuario)
        {
            Disponivel = false;
            Console.WriteLine($"[EBOOK] Link de download enviado para {usuario}.");
        }

        public override void Devolver()
        {
            Disponivel = true;
            Console.WriteLine($"[EBOOK] Acesso revogado.");
        }
        
    }

    public interface IRelatorioExcel
    {
        void GerarRelatorioExcel();
    }
     public interface IRelatorioHTML
    {
        void GerarRelatorioHTML();
    }
     public interface IRelatorioEmail
    {
        void GerarRelatorioEmail(string destinatario);
    }
     public interface IRelatorioDisco
    {
        void GerarRelatorioDisco(string caminho);
    }
     public interface IRelatorioPDF
    {
        void GerarRelatorioPDF();
    }

    public class RelatorioEmprestimos : IRelatorioPDF,IRelatorioEmail
    {
        public void GerarRelatorioPDF()
        {
            Console.WriteLine("Gerando PDF de empréstimos...");
        }

        public void EnviarPorEmail(string destinatario)
        {
            Console.WriteLine($"Enviando relatório de empréstimos para {destinatario}");
        }
    }

    public class RelatorioInventario : IRelatorioExcel
    {
        public void GerarRelatorioExcel()
        {
            Console.WriteLine("Gerando Excel de inventário...");
        }
    }


    public interface IDataBase{
        public void Salvar();
        public void Editar();
        public void Excluir();
        public void Buscar();
    }

    public class BancoDadosMySQL:IDataBase
    {
        public void Salvar(string tabela, string dados)
        {
            Console.WriteLine($"[MySQL] INSERT INTO {tabela}: {dados}");
        }

        public void Editar(){
            Console.WriteLine($"[MySQL] UPDATE {tabela} SET {coluna}={dados}");
        }
        public void Excluir(){
            Console.WriteLine($"[MySQL] Delete {tabela} WHERE {filtro}");
        }

        public List<string> Buscar(string tabela, string filtro)
        {
            Console.WriteLine($"[MySQL] SELECT * FROM {tabela} WHERE {filtro}");
            return new List<string> { "resultado simulado" };
        }
    }

    public interface ServicoEmail{
        public void Enviar();
    }

    public class ServicoEmailSMTP:ServicoEmail
    {
        public void Enviar(string para, string assunto, string corpo)
        {
            Console.WriteLine($"[SMTP] Para: {para} | Assunto: {assunto} | Corpo: {corpo}");
        }
    }

    public class GerenciarLivros
    {
        private readonly IBancoDados _banco;
        private readonly IServicoEmail _email;

        public GerenciarLivros(IBancoDados banco, IServicoEmail email)
        {
            _banco = banco;
            _email = email;
        }
        public void SalvarLivro(Livro livro)
        {
            _banco.Salvar("Livros",$"{Id}, '{Titulo}', '{Autor}', {Disponivel},{Genero},{DataEmprestimo})");
            Console.WriteLine($"Livro '{livro.Titulo}' cadastrado.");
        }

        public void NotificarAtraso(string emailUsuario, string tituloLivro, decimal multa)
        {
            _email.Enviar(emailUsuario, "Atraso na devolução",
                $"Você tem uma multa de R${multa} pelo livro '{tituloLivro}'.");
        }

        public List<string> BuscarLivrosDisponiveis()
        {
            return _banco.Buscar("livros", "disponivel = true");
        }
    }


    // =============================================================
    //   PROGRAMA PRINCIPAL — apenas demonstra o uso das classes
    // =============================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Biblioteca ===\n");

            // Demonstração das classes com violações
            var livro = new Livro
            {
                Id = 1,
                Titulo = "Clean Code",
                Autor = "Robert C. Martin",
                Genero = "Tecnologia",
                Disponivel = true,
                EmailUsuario = "aluno@faculdade.edu",
                NomeUsuario = "João Silva"
            };

            var servico = new ServicoEmprestimo();
            servico.RealizarEmprestimo(livro, "João Silva", "aluno@faculdade.edu");

            // Simulando atraso
            livro.DataEmprestimo = DateTime.Now.AddDays(-20);
            servico.DevolverLivro(livro, "Estudante");

            Console.WriteLine("\n--- Polimorfismo (com violação de LSP) ---");
            var itens = new List<ItemAcervo>
            {
                new LivroFisico { Titulo = "Design Patterns", Disponivel = true },
                new EbookEmprestavel { Titulo = "Refactoring", Disponivel = true }
            };

            foreach (var item in itens)
            {
                item.Emprestar("Maria Souza");
                try
                {
                    item.ReservarItem("Carlos"); // Vai lançar exceção no Ebook
                }
                catch (NotSupportedException ex)
                {
                    Console.WriteLine($"[ERRO] {ex.Message}");
                }
            }

            Console.WriteLine("\n--- Relatórios (com violação de ISP) ---");
            IRelatorio relEmp = new RelatorioEmprestimos();
            relEmp.GerarRelatorioPDF();
            try { relEmp.GerarRelatorioExcel(); }
            catch (NotImplementedException ex) { Console.WriteLine($"[ERRO] {ex.Message}"); }

            Console.WriteLine("\n--- Gerenciador (com violação de DIP) ---");
            var gerenciador = new GerenciadorAcervo();
            gerenciador.CadastrarLivro(livro);
            gerenciador.NotificarAtraso(livro.EmailUsuario, livro.Titulo, 15.00m);
        }
    }
}
