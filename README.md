Guía para crear una aplicación multicontenedor usando ASP.NET Core y Docker y para desplegar en un clúster Kubernetes de Azure.

La guía te enseña a:
-	Usar variables de entorno en un dockerfile, un docker compose y plantillas de Kubernetes para especificar las configuraciones de la aplicación.
-	Configurar el despliegue para monitorearlo usando Azure App Insights.
-	Delegar un dominio público a Azure DNS.

Solución Visual Studio compuesta de 2 proyectos

-	TodoWeb: ASP.NET Core web app utilizado como Front End en la solución. Proyecto compuesto de páginas Razor que pueden ser usados para ver la colección de elementos To-do almacenados en una colección de Azure Cosmos DB. El Front envía logs, eventos, traces, peticiones, dependencias y excepciones a Application Insights.
-	TodoApi: ASP-NET Core web api invocado por servicio front-end servicio para acceder al dato almacenado en un Azure Cosmo DB SQL API database. Cada vez que una operación CRUD es realizada por alguno de los métodos expuestos por el TodoController, el servicio backend envía un mensaje de notificación a una cola del Aazure Service Bus. El servicio back-end envía logs, eventos, trazas, peticiones, dependencias y excepciones a Application Insight. El backend adopta swagger para exponer los APIs.

Contenidos en Github
El código fuente está ubicado en el repositorio Git https://github.com/lurrelo/AzureK8S-multi-container-app y su contenido está estructurado de la siguiente forma: