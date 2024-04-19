using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Task
{
    public int TarefaID { get; set; }
    public string Descricao { get; set; }
    public string Estado { get; set; }
    public int ClienteID { get; set; }


    public Task(int id, string description, string status, int clientId)
    {
        TarefaID = id;
        Descricao = description;
        Estado = status;
        ClienteID = clientId;
    }
}   

class TaskManager
{
    private List<Task> tasksA;
    private List<Task> tasksB;
    private List<Task> tasksC;
    private List<Task> tasksD;
    private Mutex mutexA;
    private Mutex mutexB;
    private Mutex mutexC;
    private Mutex mutexD;
    private string csvFilePathA;
    private string csvFilePathB;
    private string csvFilePathC;
    private string csvFilePathD;

    public TaskManager(string csvFilePathA, string csvFilePathB, string csvFilePathC, string csvFilePathD)
    {
        this.csvFilePathA = csvFilePathA;
        this.csvFilePathB = csvFilePathB;
        this.csvFilePathC = csvFilePathC;
        this.csvFilePathD = csvFilePathD;
        tasksA = new List<Task>();
        tasksB = new List<Task>();
        tasksC = new List<Task>();
        tasksD = new List<Task>();
        mutexA = new Mutex();
        mutexB = new Mutex();
        mutexC = new Mutex();
        mutexD = new Mutex();
        LoadTasksFromCSV(csvFilePathA, tasksA, mutexA);
        LoadTasksFromCSV(csvFilePathB, tasksB, mutexB);
        LoadTasksFromCSV(csvFilePathC, tasksC, mutexC);
        LoadTasksFromCSV(csvFilePathD, tasksD, mutexD);
    }

    private void LoadTasksFromCSV(string csvFilePath, List<Task> tasks, Mutex mutex)
    {
        mutex.WaitOne();
        try
        {
            tasks.Clear(); // Limpa a lista de tarefas para carregar novos dados

            using (StreamReader sr = new StreamReader(csvFilePath))
            {
                sr.ReadLine(); // Lê a primeira linha (cabeçalho) 

                string line;
                while ((line = sr.ReadLine()) != null) // Lê as linhas restantes do arquivo
                {
                    string[] parts = line.Split(','); // Divide a linha em partes usando a vírgula como delimitador

                    if (parts.Length >= 4) // Verifica se há pelo menos 4 partes
                    {
                        // Extrai os dados da tarefa de cada parte
                        int TarefaID = int.Parse(parts[0]);
                        string Descricao = parts[1];
                        string Estado = parts[2];

                        int ClienteID;
                        if (int.TryParse(parts[3], out ClienteID)) // Verifica se o ID do cliente é um número válido
                        {
                            // Cria uma nova instância de tarefa e a adiciona à lista de tarefas
                            tasks.Add(new Task(TarefaID, Descricao, Estado, ClienteID));
                        }
                        else
                        {
                            Console.WriteLine($"ClienteID inválido na linha: {line}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Linha inválida no arquivo CSV: {line}");
                    }
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("Erro ao carregar tarefas do arquivo CSV: " + e.Message); // Trata exceções de E/S
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro inesperado ao carregar tarefas do arquivo CSV: " + e.Message); // Trata outras exceções
        }
        finally
        {
            mutex.ReleaseMutex(); // Libera o mutex após a conclusão da operação
        }
    }



    public void MarkTaskAsCompleted(int taskId, string csvFilePathA, string csvFilePathB, string csvFilePathC, string csvFilePathD)
    {
        try
        {
            // Verifica em qual faixa de IDs de tarefa a tarefa fornecida se encaixa e bloqueia o mutex correspondente
            if (taskId >= 1 && taskId <= 5)
            {
                mutexA.WaitOne();
                // Chama o método para marcar a tarefa como concluída no arquivo CSV correspondente ao serviço A
                MarkTaskAsCompletedInFile(tasksA, taskId, csvFilePathA);
            }
            else if (taskId >= 6 && taskId <= 10)
            {
                mutexB.WaitOne();
                // Chama o método para marcar a tarefa como concluída no arquivo CSV correspondente ao serviço B
                MarkTaskAsCompletedInFile(tasksB, taskId, csvFilePathB);
            }
            else if (taskId >= 11 && taskId <= 15) // Lidar com o serviço C
            {
                mutexC.WaitOne();
                // Chama o método para marcar a tarefa como concluída no arquivo CSV correspondente ao serviço C
                MarkTaskAsCompletedInFile(tasksC, taskId, csvFilePathC);
            }
            else if (taskId >= 16 && taskId <= 20) // Lidar com o serviço D
            {
                mutexD.WaitOne();
                // Chama o método para marcar a tarefa como concluída no arquivo CSV correspondente ao serviço D
                MarkTaskAsCompletedInFile(tasksD, taskId, csvFilePathD);
            }
            else
            {
                Console.WriteLine("ID de tarefa inválido.");
            }
        }
        finally
        {
            // Libera a mutex correspondente após a conclusão do processamento da tarefa
            if (taskId >= 1 && taskId <= 5)
            {
                mutexA.ReleaseMutex(); // Liberar a mutex para o serviço A
            }
            else if (taskId >= 6 && taskId <= 10)
            {
                mutexB.ReleaseMutex();     // Liberar a mutex para o serviço B
            }
            else if (taskId >= 11 && taskId <= 15) // Liberar a mutex para o serviço C
            {
                mutexC.ReleaseMutex();
            }
            else if (taskId >= 16 && taskId <= 20) // Liberar a mutex para o serviço D
            {
                mutexD.ReleaseMutex();
            }
        }
    }


    private void MarkTaskAsCompletedInFile(List<Task> tasks, int taskId, string csvFilePath)
    {
        Task task = tasks.FirstOrDefault(t => t.TarefaID == taskId); // Procura a tarefa na lista com base no ID

        if (task != null) // Se a tarefa for encontrada
        {
            task.Estado = "Concluido"; // Atualiza o estado da tarefa para "Concluido"
            SaveTasksToCSV(csvFilePath, tasks); // Salva as alterações no arquivo CSV
            Console.WriteLine($"Tarefa {taskId} marcada como concluída."); // Exibe uma mensagem indicando o sucesso da operação
        }
        else
        {
            Console.WriteLine($"Tarefa com ID {taskId} não encontrada."); // Se a tarefa não for encontrada, exibe uma mensagem de erro
        }
    }

    private void SaveTasksToCSV(string csvFilePath, List<Task> tasksToSave)
    {
        try
        {
            // Lista para armazenar as linhas do arquivo CSV
            List<string> lines = new List<string>();
            // Adiciona o cabeçalho das colunas no início da lista
            lines.Add("TarefaID,Descricao,Estado,ClienteID");

            // Itera sobre cada tarefa na lista de tarefas fornecida
            foreach (Task task in tasksToSave)
            {
                // Cria uma string representando a linha CSV para cada tarefa
                string line = $"{task.TarefaID},{task.Descricao},{task.Estado},{task.ClienteID}";
                // Adiciona a linha à lista de linhas do arquivo CSV
                lines.Add(line);
            }

            // Escreve todas as linhas no arquivo CSV
            File.WriteAllLines(csvFilePath, lines);
        }
        catch (Exception e)
        {
            // Captura e trata qualquer exceção que possa ocorrer durante a operação de salvamento
            Console.WriteLine("Erro ao salvar tarefas no arquivo CSV: " + e.Message);
        }
    }


    public (string, int) AssignNewTask(int clientId)
    {
        // Verifica se o cliente já possui tarefas atribuídas em algum dos serviços
        if (tasksA.Any(t => t.ClienteID == clientId && t.Estado != "Concluido") ||
            tasksB.Any(t => t.ClienteID == clientId && t.Estado != "Concluido") ||
            tasksC.Any(t => t.ClienteID == clientId && t.Estado != "Concluido") ||
            tasksD.Any(t => t.ClienteID == clientId && t.Estado != "Concluido"))
        {
            // Se o cliente já tiver tarefas atribuídas, ele ainda pode receber uma nova tarefa
            return ("Voce ja possui tarefas atribuidas.", -1);
        }

        // Procura por uma tarefa não alocada no serviço A
        Task taskA = tasksA.FirstOrDefault(t => t.Estado == "Nao alocado");
        if (taskA != null)
        {
            // Atribui a tarefa ao cliente e atualiza o estado da tarefa
            taskA.Estado = "Em curso";
            taskA.ClienteID = clientId;
            SaveTasksToCSV(csvFilePathA, tasksA); // Atualiza o CSV do serviço A
            return ($"Nova tarefa atribuida com sucesso: {taskA.Descricao}", taskA.TarefaID);
        }

        // Procura por uma tarefa não alocada no serviço B
        Task taskB = tasksB.FirstOrDefault(t => t.Estado == "Nao alocado");
        if (taskB != null)
        {
            // Atribui a tarefa ao cliente e atualiza o estado da tarefa
            taskB.Estado = "Em curso";
            taskB.ClienteID = clientId;
            SaveTasksToCSV(csvFilePathB, tasksB); // Atualiza o CSV do serviço B
            return ($"Nova tarefa atribuida com sucesso: {taskB.Descricao}", taskB.TarefaID);
        }

        // Procura por uma tarefa não alocada no serviço C
        Task taskC = tasksC.FirstOrDefault(t => t.Estado == "Nao alocado");
        if (taskC != null)
        {
            // Atribui a tarefa ao cliente e atualiza o estado da tarefa
            taskC.Estado = "Em curso";
            taskC.ClienteID = clientId;
            SaveTasksToCSV(csvFilePathC, tasksC); // Atualiza o CSV do serviço C
            return ($"Nova tarefa atribuida com sucesso: {taskC.Descricao}", taskC.TarefaID);
        }

        // Procura por uma tarefa não alocada no serviço D
        Task taskD = tasksD.FirstOrDefault(t => t.Estado == "Nao alocado");
        if (taskD != null)
        {
            // Atribui a tarefa ao cliente e atualiza o estado da tarefa
            taskD.Estado = "Em curso";
            taskD.ClienteID = clientId;
            SaveTasksToCSV(csvFilePathD, tasksD); // Atualiza o CSV do serviço D
            return ($"Nova tarefa atribuída com sucesso: {taskD.Descricao}", taskD.TarefaID);
        }

        return ("Nao ha tarefas disponiveis para atribuicao.", -1);
    }

}

class SimpleTcpServer
{
    //ID do cliente
    private static int nextClientId = 1;
    // Objeto de bloqueio para garantir a exclusão mútua na geração do ID do cliente
    private static readonly object clientLock = new object();

    // Método para gerar o próximo ID de cliente de forma exclusiva
    private static int GenerateClientId()
    {
        lock (clientLock)
        {
            return nextClientId++;
        }
    }

    static void Main()
    {
        // Caminhos dos arquivos CSV para os serviços A, B, C e D
        string csvFilePath1 = "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_A.csv";
        string csvFilePath2 = "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_B.csv";
        string csvFilePath3 = "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_C.csv";
        string csvFilePath4 = "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_D.csv";

        // Inicialização do csv
        TaskManager taskManager = new TaskManager(csvFilePath1, csvFilePath2, csvFilePath3, csvFilePath4);

        TcpListener server = null;
        try
        {
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, port);
            server.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                // Aceita conexões de clientes e cria uma instância TcpClient para cada cliente conectado
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected...");

                // Gera um ID único para o cliente
                int clientId = GenerateClientId();

                // Cria uma nova thread para lidar com o cliente
                Thread clientThread = new Thread(() => HandleClient(client, clientId, taskManager, csvFilePath1, csvFilePath2, csvFilePath3, csvFilePath4));
                clientThread.Start();
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            // Encerra o servidor
            server?.Stop();
        }

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }


static void HandleClient(TcpClient client, int clientId, TaskManager taskManager, string csvFilePathA, string csvFilePathB, string csvFilePathC, string csvFilePathD)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            // Envia uma mensagem inicial de confirmação ao cliente
            byte[] initialResponse = Encoding.ASCII.GetBytes("100 OK");
            stream.Write(initialResponse, 0, initialResponse.Length);
            Console.WriteLine("Sent: 100 OK");

            // Envia a ID do cliente ao cliente para identificação
            string idResponse = $"ID:{clientId} \n";
            byte[] idResponseBytes = Encoding.ASCII.GetBytes(idResponse);
            stream.Write(idResponseBytes, 0, idResponseBytes.Length);
            Console.WriteLine("Sent: " + idResponse);

            // Buffer para armazenar os dados recebidos do cliente
            byte[] buffer = new byte[1024];
            int bytesRead;

            // Loop para receber mensagens do cliente continuamente
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                // Converte os dados recebidos em uma string
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + message);

                // Verifica se o cliente solicitou encerrar a conexão
                if (message.Trim().ToUpper() == "QUIT")
                {
                    // Envia uma mensagem de despedida ao cliente e encerra o loop
                    byte[] responseMessage = Encoding.ASCII.GetBytes("400 BYE");
                    stream.Write(responseMessage, 0, responseMessage.Length);
                    Console.WriteLine("Sent: 400 BYE");
                    break;
                }
                // Verifica se o cliente notificou a conclusão de uma tarefa
                else if (message.StartsWith("CONCLUIDA"))
                {
                    // Extrai o ID da tarefa da mensagem do cliente
                    int taskId = int.Parse(message.Substring(10));
                    // Determina o arquivo CSV correto com base no ID do cliente
                    string csvFilePath = GetCsvFilePathForClient(clientId);
                    // Marca a tarefa como concluída no csv
                    taskManager.MarkTaskAsCompleted(taskId, csvFilePathA, csvFilePathB, csvFilePathC, csvFilePathD);

                    // Envia uma mensagem de confirmação ao cliente
                    byte[] responseMessage = Encoding.ASCII.GetBytes("Tarefa concluída com sucesso.");
                    stream.Write(responseMessage, 0, responseMessage.Length);
                    Console.WriteLine("Sent: Tarefa concluída com sucesso.");
                }
                // Verifica se o cliente solicitou uma nova tarefa
                else if (message == "NOVA_TAREFA")
                {
                    // Atribui uma nova tarefa ao cliente no csv
                    (string taskMessage, int taskId) = taskManager.AssignNewTask(clientId);
                    string response = taskMessage + "   || ID da tarefa: " + taskId;
                    // Envia a resposta ao cliente
                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] responseMessage = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseMessage, 0, responseMessage.Length);
                        Console.WriteLine("Sent: " + response);
                    }
                }

                // Limpa o buffer para a próxima iteração
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        // Captura exceções durante a comunicação com o cliente
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred with client {clientId}: {ex.Message}");
        }
        // Finaliza a conexão com o cliente após a conclusão da comunicação
        finally
        {
            client.Close();
            Console.WriteLine($"Client {clientId} disconnected.");
        }
    }


    private static string GetCsvFilePathForClient(int clientId)
    {
        if (clientId % 4 == 0)
        {
            return "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_A.csv";
        }
        else if (clientId % 4 == 1)
        {
            return "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_B.csv";
        }
        else if (clientId % 4 == 2)
        {
            return "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_C.csv";
        }
        else 
        {
            return "C:/Users/Moisés/Desktop/Universidade/3º ano/2º Semestre/Sistemas Distribuídos/Ficheiros de exemplo/Servico_D.csv";
        }
    }
}