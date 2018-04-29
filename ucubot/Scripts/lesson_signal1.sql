ALTER TABLE lesson_signal ADD student_id INT NOT NULL;
ALTER TABLE lesson_signal ADD CONSTRAINT fk_student_id FOREIGN KEY (student_id) REFERENCES student(id);
