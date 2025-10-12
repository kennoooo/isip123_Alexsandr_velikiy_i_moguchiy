using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversityApp
{
    abstract class Person
    {
        public int Id { get; }
        public string Name { get; private set; }
        public int Age { get; private set; }
        public string Contact { get; private set; }
        protected Person(int id, string name, int age, string contact)
        { Id = id; Name = name; Age = age; Contact = contact; }
        public abstract string GetInfo();
    }

    class Student : Person
    {
        private readonly HashSet<int> courseIds = new();
        public Student(int id, string name, int age, string contact) : base(id, name, age, contact) { }
        public void Enroll(int courseId) { if (!courseIds.Add(courseId)) throw new InvalidOperationException("Студент уже записан на курс."); }
        public void Unenroll(int courseId) { courseIds.Remove(courseId); }
        public IEnumerable<int> Courses => courseIds;
        public override string GetInfo() => $"Студент #{Id}: {Name}, {Age} лет, контакт: {Contact}";
    }

    class Teacher : Person
    {
        private readonly HashSet<int> courseIds = new();
        public Teacher(int id, string name, int age, string contact) : base(id, name, age, contact) { }
        public void AssignCourse(int courseId) => courseIds.Add(courseId);
        public void RemoveCourse(int courseId) => courseIds.Remove(courseId);
        public IEnumerable<int> Courses => courseIds;
        public override string GetInfo() => $"Преподаватель #{Id}: {Name}, {Age} лет, контакт: {Contact}";
    }

    class Course
    {
        public int Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int? TeacherId { get; private set; }
        private readonly HashSet<int> students = new();
        public Course(int id, string title, string description = "") { Id = id; Title = title; Description = description; }
        public void SetTeacher(int? tId) => TeacherId = tId;
        public void AddStudent(int sId) { if (!students.Add(sId)) throw new InvalidOperationException("Студент уже на курсе."); }
        public void RemoveStudent(int sId) => students.Remove(sId);
        public IEnumerable<int> Students => students;
        public string GetInfo()
        {
            var t = TeacherId.HasValue ? TeacherId.Value.ToString() : "не назначен";
            return $"Курс #{Id}: {Title}\nОписание: {Description}\nПреподаватель ID: {t}\nСтудентов: {students.Count}";
        }
    }

    class UniversitySystem
    {
        private readonly Dictionary<int, Student> students = new();
        private readonly Dictionary<int, Teacher> teachers = new();
        private readonly Dictionary<int, Course> courses = new();
        private int nextStudent = 1, nextTeacher = 1, nextCourse = 1;

        // Добавление
        public int AddStudent(string name, int age, string contact)
        {
            var id = nextStudent++;
            students[id] = new Student(id, name, age, contact);
            return id;
        }
        public int AddTeacher(string name, int age, string contact)
        {
            var id = nextTeacher++;
            teachers[id] = new Teacher(id, name, age, contact);
            return id;
        }
        public int AddCourse(string title, string desc = "", int? teacherId = null)
        {
            var id = nextCourse++;
            var c = new Course(id, title, desc);
            if (teacherId.HasValue)
            {
                if (!teachers.ContainsKey(teacherId.Value)) throw new KeyNotFoundException("Преподаватель не найден.");
                c.SetTeacher(teacherId.Value);
                teachers[teacherId.Value].AssignCourse(id);
            }
            courses[id] = c;
            return id;
        }

        // Получение
        public Student GetStudent(int id) => students.TryGetValue(id, out var s) ? s : throw new KeyNotFoundException("Студент не найден.");
        public Teacher GetTeacher(int id) => teachers.TryGetValue(id, out var t) ? t : throw new KeyNotFoundException("Преподаватель не найден.");
        public Course GetCourse(int id) => courses.TryGetValue(id, out var c) ? c : throw new KeyNotFoundException("Курс не найден.");

        // Удаление с каскадом
        public void RemoveStudent(int id)
        {
            if (!students.ContainsKey(id)) throw new KeyNotFoundException("Студент не найден.");
            // удалить студента из всех курсов
            foreach (var c in courses.Values) c.RemoveStudent(id);
            students.Remove(id);
        }

        public void RemoveTeacher(int id)
        {
            if (!teachers.ContainsKey(id)) throw new KeyNotFoundException("Преподаватель не найден.");
            // отвязать преподавателя от курсов
            foreach (var c in courses.Values.Where(x => x.TeacherId == id)) c.SetTeacher(null);
            teachers.Remove(id);
        }

        public void RemoveCourse(int id)
        {
            if (!courses.ContainsKey(id)) throw new KeyNotFoundException("Курс не найден.");
            // удалить курс из студентов и у преподавателя
            foreach (var s in students.Values) s.Unenroll(id);
            var tId = courses[id].TeacherId;
            if (tId.HasValue && teachers.ContainsKey(tId.Value)) teachers[tId.Value].RemoveCourse(id);
            courses.Remove(id);
        }

        // Операции
        public void AssignTeacher(int courseId, int teacherId)
        {
            var c = GetCourse(courseId);
            var t = GetTeacher(teacherId);
            // отвязать старого
            if (c.TeacherId.HasValue && teachers.ContainsKey(c.TeacherId.Value)) teachers[c.TeacherId.Value].RemoveCourse(courseId);
            c.SetTeacher(teacherId);
            t.AssignCourse(courseId);
        }

        public void EnrollStudent(int studentId, int courseId)
        {
            var s = GetStudent(studentId);
            var c = GetCourse(courseId);
            c.AddStudent(studentId);
            s.Enroll(courseId);
        }

        // Единая функция вывода всех данных
        public void PrintAllData()
        {
            Console.WriteLine("\n--- Все студенты ---");
            if (!students.Any()) Console.WriteLine("(Нет студентов)");
            foreach (var s in students.Values) Console.WriteLine(s.GetInfo() + "\nКурсы: " + (s.Courses.Any() ? string.Join(", ", s.Courses) : "—"));

            Console.WriteLine("\n--- Все преподаватели ---");
            if (!teachers.Any()) Console.WriteLine("(Нет преподавателей)");
            foreach (var t in teachers.Values) Console.WriteLine(t.GetInfo() + "\nКурсы: " + (t.Courses.Any() ? string.Join(", ", t.Courses) : "—"));

            Console.WriteLine("\n--- Все курсы ---");
            if (!courses.Any()) Console.WriteLine("(Нет курсов)");
            foreach (var c in courses.Values)
            {
                Console.WriteLine(c.GetInfo());
                if (c.Students.Any()) Console.WriteLine("Список студентов (ID): " + string.Join(", ", c.Students));
                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var sys = new UniversitySystem();

            // Пример данных
            var tid = sys.AddTeacher("Иван Петров", 45, "ivan@uni.ru");
            var cid = sys.AddCourse("Введение в C#", "Основы языка", tid);
            var sid = sys.AddStudent("Анна Смирнова", 20, "anna@student.ru");
            sys.EnrollStudent(sid, cid);

            while (true)
            {
                Console.WriteLine("\n=== Меню ===");
                Console.WriteLine("1) Добавить/Удалить (студент / преподаватель / курс)");
                Console.WriteLine("2) Показать всё (вызов одной функции)");
                Console.WriteLine("3) Назначить преподавателя на курс");
                Console.WriteLine("4) Записать студента на курс");
                Console.WriteLine("0) Выход");
                Console.Write("Выберите пункт: ");
                var choice = Console.ReadLine();

                try
                {
                    if (choice == "0") { Console.WriteLine("Выход. До свидания!"); break; }

                    if (choice == "1")
                    {
                        Console.Write("Введите действие (добавить / удалить): ");
                        var act = Console.ReadLine()?.Trim().ToLower();
                        Console.Write("Тип (student / teacher / course): ");
                        var type = Console.ReadLine()?.Trim().ToLower();

                        if (act == "добавить")
                        {
                            if (type == "student")
                            {
                                Console.Write("Имя: "); var n = Console.ReadLine();
                                Console.Write("Возраст: "); var a = int.Parse(Console.ReadLine() ?? "0");
                                Console.Write("Контакт: "); var c = Console.ReadLine();
                                Console.WriteLine($"Добавлен студент ID = {sys.AddStudent(n, a, c)}");
                            }
                            else if (type == "teacher")
                            {
                                Console.Write("Имя: "); var n = Console.ReadLine();
                                Console.Write("Возраст: "); var a = int.Parse(Console.ReadLine() ?? "0");
                                Console.Write("Контакт: "); var c = Console.ReadLine();
                                Console.WriteLine($"Добавлен преподаватель ID = {sys.AddTeacher(n, a, c)}");
                            }
                            else if (type == "course")
                            {
                                Console.Write("Название: "); var t = Console.ReadLine();
                                Console.Write("Описание: "); var d = Console.ReadLine();
                                Console.Write("ID преподавателя (или пусто): "); var r = Console.ReadLine();
                                int? tt = int.TryParse(r, out var num) ? num : null;
                                Console.WriteLine($"Добавлен курс ID = {sys.AddCourse(t, d, tt)}");
                            }
                            else Console.WriteLine("Неизвестный тип.");
                        }
                        else if (act == "удалить")
                        {
                            if (type == "student")
                            {
                                Console.Write("ID студента для удаления: "); var id = int.Parse(Console.ReadLine() ?? "0");
                                sys.RemoveStudent(id); Console.WriteLine("Студент удалён.");
                            }
                            else if (type == "teacher")
                            {
                                Console.Write("ID преподавателя для удаления: "); var id = int.Parse(Console.ReadLine() ?? "0");
                                sys.RemoveTeacher(id); Console.WriteLine("Преподаватель удалён (курсы отвязаны).");
                            }
                            else if (type == "course")
                            {
                                Console.Write("ID курса для удаления: "); var id = int.Parse(Console.ReadLine() ?? "0");
                                sys.RemoveCourse(id); Console.WriteLine("Курс удалён.");
                            }
                            else Console.WriteLine("Неизвестный тип.");
                        }
                        else Console.WriteLine("Неизвестное действие.");
                    }
                    else if (choice == "2")
                    {
                        sys.PrintAllData(); // единая функция вывода
                    }
                    else if (choice == "3")
                    {
                        Console.Write("ID курса: "); var cidn = int.Parse(Console.ReadLine() ?? "0");
                        Console.Write("ID преподавателя: "); var tidn = int.Parse(Console.ReadLine() ?? "0");
                        sys.AssignTeacher(cidn, tidn);
                        Console.WriteLine("Преподаватель назначен.");
                    }
                    else if (choice == "4")
                    {
                        Console.Write("ID студента: "); var sidn = int.Parse(Console.ReadLine() ?? "0");
                        Console.Write("ID курса: "); var cidn = int.Parse(Console.ReadLine() ?? "0");
                        sys.EnrollStudent(sidn, cidn);
                        Console.WriteLine("Студент записан на курс.");
                    }
                    else Console.WriteLine("Неверный выбор.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }
        }
    }
}
