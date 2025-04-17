


from django.shortcuts import get_object_or_404, redirect, render
from .forms import bookForm,CategoryForm
from .models import Book, Category


# Create your views here.
def index(request):
     if request.method == 'POST':
        add_book = bookForm(request.POST, request.FILES)
        if add_book.is_valid():
           add_book.save()
        else:
              print("❌ أخطاء في نموذج الكتاب:", add_book.errors)

        add_category = CategoryForm(request.POST, request.FILES)
        if add_category.is_valid():
           add_category.save()
        else:
              print("❌ أخطاء في نموذج الكتاب:", add_category.errors)
     context = {   
          'books':Book.objects.all(),
          'categories':Category.objects.all(),
          'form' : bookForm(),
          'categoryForm' : CategoryForm(),
          'allbooks' : Book.objects.filter(active=True).count,
          'booksSoled' : Book.objects.filter(status='sold').count,
          'booksRental' : Book.objects.filter(status='rental').count,
          'booksAvailable' : Book.objects.filter(status='available').count
          }
     
     return render(request,'pages/index.html',context)

def books(request):
     context = {   
          'books':Book.objects.all(),
          'categories':Category.objects.all(),
          }
     
     return render(request,'pages/books.html',context)

def update(request, id):
    book = get_object_or_404(Book, id=id)

    if request.method == 'POST':
        edit_book = bookForm(request.POST, request.FILES, instance=book)
        if edit_book.is_valid():
            edit_book.save()
            return redirect('/') 
        else:
            print("❌ أخطاء في نموذج الكتاب:", edit_book.errors)
    else:
        edit_book = bookForm(instance=book)

    return render(request, 'pages/update.html', {'form': edit_book, 'book': book})

def delete(request, id):
    book = get_object_or_404(Book, id=id)

    if request.method == 'POST':
        book.delete()
        return redirect('/')  # or redirect to your book list page

    return render(request, 'pages/delete.html', {'book': book})
